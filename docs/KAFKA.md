# Kafka cho dotnet-platform-api

Tài liệu này chỉ hướng dẫn phần **C# .NET 8** cần code Kafka như thế nào, theo thứ tự triển khai trong `backend/dotnet-platform-api`.

Mục tiêu của phía .NET:

- Consume `OrderCreated` từ `order.events` để tạo payment order nội bộ.
- Publish `PaymentSucceeded` lên `payment.events` sau khi PayOS webhook báo thanh toán thành công.
- Không update bảng của Java trực tiếp. .NET chỉ ghi bảng của .NET và phát event cho service khác xử lý.

Contract event lấy theo source of truth trong [`contracts/kafka`](../contracts/kafka):

- [`OrderCreated.schema.json`](../contracts/kafka/OrderCreated.schema.json)
- [`PaymentSucceeded.schema.json`](../contracts/kafka/PaymentSucceeded.schema.json)

Lưu ý quan trọng: schema hiện tại là JSON phẳng, không có wrapper `data`.

Ví dụ `OrderCreated`:

```json
{
  "eventId": "4ef315dc-f7c9-4c08-9d0b-74a5e49f84fa",
  "eventType": "OrderCreated",
  "occurredAt": "2026-07-10T10:30:00Z",
  "orderId": "a08f5c1d-623c-44c4-a61f-31ce0f305c65",
  "orderCode": "1752143400001",
  "customerId": "53bbcf63-9337-4c68-8f0f-bb277f2ad5dd",
  "totalAmount": 250000,
  "currency": "VND"
}
```

Ví dụ `PaymentSucceeded`:

```json
{
  "eventId": "d7900e86-1f89-4f56-a1a2-c7c197857c0a",
  "eventType": "PaymentSucceeded",
  "occurredAt": "2026-07-10T10:35:00Z",
  "paymentId": "7c6068d0-60e2-41c7-a274-7983bc3ec377",
  "orderId": "a08f5c1d-623c-44c4-a61f-31ce0f305c65",
  "customerId": "53bbcf63-9337-4c68-8f0f-bb277f2ad5dd",
  "amount": 250000,
  "paymentMethod": "MOCK",
  "transactionCode": "PAYOS-123",
  "invoiceNumber": "INV202607100001",
  "invoicePdfUrl": "http://localhost:19000/invoices/INV202607100001.pdf"
}
```

---

## 0. Luồng cần đạt

Luồng đúng của .NET trong MVP:

```text
Java Order Service
  -> publish OrderCreated vào order.events

dotnet-platform-api
  -> consume OrderCreated
  -> tạo PaymentOrder PENDING
  -> tạo payment link PayOS nếu cần

PayOS webhook
  -> POST /api/payments/webhook vào dotnet-platform-api
  -> verify webhook
  -> mark PaymentOrder = PAID
  -> tạo invoice + PDF
  -> publish PaymentSucceeded vào payment.events

Java Order Service
  -> consume PaymentSucceeded
  -> update order PAID + lock design
```

Thứ tự code nên làm:

1. Sửa config Kafka.
2. Cài package Kafka cho Infrastructure.
3. Tạo `KafkaSettings`.
4. Tạo event DTO đúng schema.
5. Tạo `IEventPublisher`.
6. Implement `KafkaEventPublisher`.
7. Đăng ký DI.
8. Publish `PaymentSucceeded` trong webhook handler.
9. Tạo command xử lý `OrderCreated`.
10. Tạo `OrderCreatedConsumer` background service.
11. Test bằng Kafka UI hoặc console producer.

---

## 1. Sửa config Kafka

File: `backend/dotnet-platform-api/AiFashionStudio.Platform.Api/appsettings.json`

Hiện tại đang có:

```json
"Kafka": {
  "BootstrapServers": "localhost:29092"
}
```

Sửa thành:

```json
"Kafka": {
  "BootstrapServers": "localhost:39092",
  "GroupId": "platform-payment-service",
  "OrderEventsTopic": "order.events",
  "PaymentEventsTopic": "payment.events"
}
```

Giải thích port:

- App .NET chạy trực tiếp trên máy host dùng `localhost:39092`.
- App .NET chạy trong Docker cùng network với Kafka dùng `kafka:9092`.
- Không dùng `localhost:29092` cho app chạy trên host.

Nếu có `appsettings.Development.json`, có thể override tại đó:

```json
"Kafka": {
  "BootstrapServers": "localhost:39092",
  "GroupId": "platform-payment-service",
  "OrderEventsTopic": "order.events",
  "PaymentEventsTopic": "payment.events"
}
```

---

## 2. Cài package Kafka

Chạy trong thư mục:

```powershell
cd backend/dotnet-platform-api
dotnet add AiFashionStudio.Platform.Infrastructure package Confluent.Kafka
```

Package đặt ở Infrastructure vì Kafka là chi tiết hạ tầng, không để Application phụ thuộc trực tiếp vào `Confluent.Kafka`.

---

## 3. Tạo KafkaSettings

Tạo file:

`AiFashionStudio.Platform.Infrastructure/Kafka/KafkaSettings.cs`

```csharp
namespace AiFashionStudio.Platform.Infrastructure.Kafka;

public sealed class KafkaSettings
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; init; } = string.Empty;
    public string GroupId { get; init; } = "platform-payment-service";
    public string OrderEventsTopic { get; init; } = "order.events";
    public string PaymentEventsTopic { get; init; } = "payment.events";
}
```

---

## 4. Tạo event DTO đúng schema

DTO nên đặt ở Application vì handler cần tạo/đọc event nhưng không cần biết Kafka library.

Tạo folder:

`AiFashionStudio.Platform.Application/Common/Events`

Tạo file:

`AiFashionStudio.Platform.Application/Common/Events/OrderCreatedEvent.cs`

```csharp
namespace AiFashionStudio.Platform.Application.Common.Events;

public sealed record OrderCreatedEvent(
    Guid EventId,
    string EventType,
    DateTime OccurredAt,
    Guid OrderId,
    string OrderCode,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency = "VND");
```

Tạo file:

`AiFashionStudio.Platform.Application/Common/Events/PaymentSucceededEvent.cs`

```csharp
namespace AiFashionStudio.Platform.Application.Common.Events;

public sealed record PaymentSucceededEvent(
    Guid EventId,
    string EventType,
    DateTime OccurredAt,
    Guid PaymentId,
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string PaymentMethod,
    string TransactionCode,
    string? InvoiceNumber,
    string? InvoicePdfUrl);
```

Quy ước:

- `EventType` phải đúng contract: `OrderCreated`, `PaymentSucceeded`.
- `EventId` tạo mới bằng `Guid.NewGuid()`.
- `OccurredAt` dùng `DateTime.UtcNow`.
- Message key khi publish nên là `orderId.ToString()` để giữ thứ tự event theo order.

---

## 5. Tạo IEventPublisher

Tạo file:

`AiFashionStudio.Platform.Application/Common/Interfaces/IServices/IEventPublisher.cs`

```csharp
namespace AiFashionStudio.Platform.Application.Common.Interfaces.IServices;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(
        string topic,
        string key,
        TEvent @event,
        CancellationToken cancellationToken = default);
}
```

Application chỉ biết interface này. Infrastructure sẽ implement bằng Kafka.

---

## 6. Implement KafkaEventPublisher

Tạo file:

`AiFashionStudio.Platform.Infrastructure/Kafka/KafkaEventPublisher.cs`

```csharp
using System.Text.Json;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace AiFashionStudio.Platform.Infrastructure.Kafka;

public sealed class KafkaEventPublisher : IEventPublisher, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IProducer<string, string> _producer;

    public KafkaEventPublisher(IOptions<KafkaSettings> options)
    {
        var settings = options.Value;

        var config = new ProducerConfig
        {
            BootstrapServers = settings.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync<TEvent>(
        string topic,
        string key,
        TEvent @event,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(@event, JsonOptions);

        await _producer.ProduceAsync(
            topic,
            new Message<string, string>
            {
                Key = key,
                Value = json
            },
            cancellationToken);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
```

Vì dùng `JsonSerializerDefaults.Web`, JSON output sẽ là camelCase:

- `EventId` -> `eventId`
- `EventType` -> `eventType`
- `PaymentId` -> `paymentId`

Khớp contract trong `contracts/kafka`.

---

## 7. Đăng ký DI

Sửa file:

`AiFashionStudio.Platform.Infrastructure/DependencyInjection.cs`

Thêm using:

```csharp
using AiFashionStudio.Platform.Infrastructure.Kafka;
```

Trong `AddInfrastructure`, thêm:

```csharp
services.Configure<KafkaSettings>(configuration.GetSection(KafkaSettings.SectionName));
services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
```

Sau này khi tạo consumer thì thêm:

```csharp
services.AddHostedService<OrderCreatedConsumer>();
```

Nhưng chưa thêm consumer trước khi tạo class consumer ở bước 10.

---

## 8. Publish PaymentSucceeded trong webhook

File cần sửa:

`AiFashionStudio.Platform.Application/Payments/Commands/ProcessPaymentWebhook/ProcessPaymentWebhookCommandHandler.cs`

Hiện handler đang làm:

1. Verify PayOS webhook.
2. Tìm payment order.
3. Check success.
4. Check amount.
5. Mark paid.
6. Save.
7. Tạo invoice.
8. Generate PDF.

Sau bước invoice/PDF, publish `PaymentSucceeded`.

### 8.1 Inject thêm dependency

Thêm field:

```csharp
private readonly IEventPublisher _eventPublisher;
```

Nếu cần topic từ config, inject:

```csharp
private readonly KafkaSettings _kafkaSettings;
```

Nhưng Application không nên phụ thuộc `KafkaSettings` ở Infrastructure. Cách sạch hơn là tạo interface topic options trong Application hoặc truyền topic qua publisher wrapper. Với MVP có thể làm đơn giản: tạo constant topic trong handler:

```csharp
private const string PaymentEventsTopic = "payment.events";
```

Constructor thêm:

```csharp
IEventPublisher eventPublisher
```

### 8.2 Tạo event sau invoice

Sau đoạn:

```csharp
if (invoiceId is not null)
{
    await _sender.Send(new GenerateInvoicePdfCommand(invoiceId.Value), cancellationToken);
}
```

Thêm:

```csharp
var paymentSucceeded = new PaymentSucceededEvent(
    EventId: Guid.NewGuid(),
    EventType: "PaymentSucceeded",
    OccurredAt: DateTime.UtcNow,
    PaymentId: order.Id,
    OrderId: order.Id,
    CustomerId: order.UserId,
    Amount: order.Amount,
    PaymentMethod: "MOCK",
    TransactionCode: webhook.Reference,
    InvoiceNumber: null,
    InvoicePdfUrl: null);

await _eventPublisher.PublishAsync(
    PaymentEventsTopic,
    key: order.Id.ToString(),
    paymentSucceeded,
    cancellationToken);
```

### 8.3 Điểm cần chỉnh cho đúng flow thật

Hiện `PaymentOrder` của .NET đang dùng `order.Id` như payment id và order id. Với flow Kafka chuẩn, nên tách rõ:

- `PaymentOrder.Id`: id payment record bên .NET.
- `PaymentOrder.OrderId`: id order từ Java.
- `PaymentOrder.CustomerId` hoặc `UserId`: id customer.
- `PaymentOrder.OrderCode`: mã PayOS/orderCode.

Vì contract `PaymentSucceeded` yêu cầu cả `paymentId` và `orderId`, nên nên thêm cột `order_id` vào `PaymentOrder`.

Sau khi có `PaymentOrder.OrderId`, event phải publish như sau:

```csharp
var paymentSucceeded = new PaymentSucceededEvent(
    EventId: Guid.NewGuid(),
    EventType: "PaymentSucceeded",
    OccurredAt: DateTime.UtcNow,
    PaymentId: order.Id,
    OrderId: order.OrderId,
    CustomerId: order.UserId,
    Amount: order.Amount,
    PaymentMethod: "MOCK",
    TransactionCode: webhook.Reference,
    InvoiceNumber: invoice?.InvoiceNumber,
    InvoicePdfUrl: invoice?.PdfUrl);
```

Nếu chưa có `invoiceNumber` và `invoicePdfUrl` ngay lúc publish, có 2 lựa chọn:

- Publish `PaymentSucceeded` sau khi query lại invoice.
- Cho phép `invoiceNumber` và `invoicePdfUrl` là `null` như schema đã cho phép.

---

## 9. Tạo command xử lý OrderCreated

Mục tiêu: khi .NET nhận event `OrderCreated`, tạo `PaymentOrder` tương ứng nếu chưa có.

Tạo folder:

`AiFashionStudio.Platform.Application/Payments/Commands/HandleOrderCreated`

Tạo file:

`HandleOrderCreatedCommand.cs`

```csharp
using AiFashionStudio.Platform.Application.Common.Events;
using MediatR;

namespace AiFashionStudio.Platform.Application.Payments.Commands.HandleOrderCreated;

public sealed record HandleOrderCreatedCommand(OrderCreatedEvent Event) : IRequest;
```

Tạo file:

`HandleOrderCreatedCommandHandler.cs`

```csharp
using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using AiFashionStudio.Platform.Application.Common.Models;
using AiFashionStudio.Platform.Domain.Payment.Entities;
using MediatR;

namespace AiFashionStudio.Platform.Application.Payments.Commands.HandleOrderCreated;

public sealed class HandleOrderCreatedCommandHandler : IRequestHandler<HandleOrderCreatedCommand>
{
    private readonly IPaymentOrderRepository _paymentOrderRepository;
    private readonly IPaymentGatewayService _paymentGatewayService;

    public HandleOrderCreatedCommandHandler(
        IPaymentOrderRepository paymentOrderRepository,
        IPaymentGatewayService paymentGatewayService)
    {
        _paymentOrderRepository = paymentOrderRepository;
        _paymentGatewayService = paymentGatewayService;
    }

    public async Task Handle(HandleOrderCreatedCommand command, CancellationToken cancellationToken)
    {
        var orderCodeText = command.Event.OrderCode;
        if (!long.TryParse(orderCodeText, out var orderCode))
        {
            throw new AppValidationException("orderCode", "ORDER_CODE_INVALID", "Order code must be numeric for PayOS.");
        }

        var existing = await _paymentOrderRepository.GetByOrderCodeAsync(orderCode, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        var amount = decimal.ToInt32(command.Event.TotalAmount);

        var payment = PaymentOrder.Create(
            command.Event.CustomerId,
            orderCode,
            amount,
            $"Order {command.Event.OrderCode}");

        await _paymentOrderRepository.AddAsync(payment, cancellationToken);

        var link = await _paymentGatewayService.CreatePaymentLinkAsync(
            new PaymentLinkRequest(
                orderCode,
                amount,
                payment.Description,
                ReturnUrl: "",
                CancelUrl: ""),
            cancellationToken);

        payment.AttachPaymentLink(link.PaymentLink);
        await _paymentOrderRepository.SaveChangesAsync(cancellationToken);
    }
}
```

Điểm cần làm tốt hơn sau MVP:

- Thêm `OrderId` vào `PaymentOrder.Create(...)` để lưu id order Java.
- Check idempotency bằng `orderId` thay vì chỉ `orderCode`.
- Có bảng `processed_events` để chống xử lý trùng theo `eventId`.

---

## 10. Tạo OrderCreatedConsumer

Consumer là hosted service chạy nền trong API.

Tạo file:

`AiFashionStudio.Platform.Infrastructure/Kafka/OrderCreatedConsumer.cs`

```csharp
using System.Text.Json;
using AiFashionStudio.Platform.Application.Common.Events;
using AiFashionStudio.Platform.Application.Payments.Commands.HandleOrderCreated;
using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiFashionStudio.Platform.Infrastructure.Kafka;

public sealed class OrderCreatedConsumer : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaSettings _settings;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaSettings> options,
        ILogger<OrderCreatedConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = options.Value;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);
    }

    private void ConsumeLoop(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(_settings.OrderEventsTopic);

        while (!stoppingToken.IsCancellationRequested)
        {
            ConsumeResult<string, string>? result = null;

            try
            {
                result = consumer.Consume(stoppingToken);
                var message = result.Message.Value;

                var orderCreated = JsonSerializer.Deserialize<OrderCreatedEvent>(message, JsonOptions);
                if (orderCreated is null || orderCreated.EventType != "OrderCreated")
                {
                    consumer.Commit(result);
                    continue;
                }

                using var scope = _scopeFactory.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                sender.Send(new HandleOrderCreatedCommand(orderCreated), stoppingToken)
                    .GetAwaiter()
                    .GetResult();

                consumer.Commit(result);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Invalid Kafka message on topic {Topic}", _settings.OrderEventsTopic);

                if (result is not null)
                {
                    consumer.Commit(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to consume OrderCreated event");
            }
        }

        consumer.Close();
    }
}
```

Vì `BackgroundService.ExecuteAsync` là async nhưng `Confluent.Kafka` consumer API chính là blocking, cách trên bọc loop trong `Task.Run`. Đủ ổn cho MVP local.

Quy tắc commit:

- Xử lý thành công -> commit.
- Message JSON sai -> log rồi commit để không kẹt mãi.
- Lỗi DB/gateway -> không commit để Kafka retry sau.

---

## 11. Đăng ký consumer

Sửa file:

`AiFashionStudio.Platform.Infrastructure/DependencyInjection.cs`

Sau khi đã tạo `OrderCreatedConsumer`, thêm:

```csharp
services.AddHostedService<OrderCreatedConsumer>();
```

Kết quả phần Kafka trong DI nên giống:

```csharp
services.Configure<KafkaSettings>(configuration.GetSection(KafkaSettings.SectionName));
services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
services.AddHostedService<OrderCreatedConsumer>();
```

---

## 12. Cập nhật PaymentOrder để flow đúng hơn

Phần này nên làm trước khi nối Java thật.

Hiện `PaymentOrder.Create(...)` chỉ nhận:

```csharp
PaymentOrder.Create(Guid userId, long orderCode, int amount, string description)
```

Nên thêm `OrderId`:

```csharp
public Guid OrderId { get; private set; }
```

Sửa factory:

```csharp
public static PaymentOrder Create(Guid userId, Guid orderId, long orderCode, int amount, string description)
{
    return new()
    {
        UserId = userId,
        OrderId = orderId,
        OrderCode = orderCode,
        Amount = amount,
        Description = description
    };
}
```

Cập nhật EF config:

```csharp
builder.Property(order => order.OrderId)
    .HasColumnName("order_id")
    .IsRequired();

builder.HasIndex(order => order.OrderId).IsUnique();
```

Tạo migration:

```powershell
dotnet ef migrations add AddOrderIdToPaymentOrder --project AiFashionStudio.Platform.Infrastructure --startup-project AiFashionStudio.Platform.Api
```

Sau đó các chỗ gọi `PaymentOrder.Create(...)` phải truyền thêm `orderId`.

---

## 13. Thứ tự test sau mỗi bước

### 13.1 Test compile

```powershell
cd backend/dotnet-platform-api
dotnet build AiFashionStudio.Platform.sln
```

Nếu máy bị lỗi quyền NuGet config, dùng:

```powershell
dotnet build AiFashionStudio.Platform.sln --no-restore
```

### 13.2 Test unit

```powershell
dotnet test AiFashionStudio.Platform.sln --no-restore
```

Nên thêm test cho:

- `HandleOrderCreatedCommandHandler`: event mới tạo payment order.
- `HandleOrderCreatedCommandHandler`: event trùng không tạo payment order lần 2.
- `ProcessPaymentWebhookCommandHandler`: webhook success publish `PaymentSucceeded`.
- `ProcessPaymentWebhookCommandHandler`: amount mismatch không publish event.

### 13.3 Chạy Kafka local

```powershell
cd infra
docker compose up -d kafka kafka-init kafka-ui
```

Mở Kafka UI:

```text
http://localhost:18085
```

Kiểm tra topic:

- `order.events`
- `payment.events`

### 13.4 Test consumer OrderCreated bằng Kafka UI

Vào Kafka UI:

1. Chọn topic `order.events`.
2. Chọn produce message.
3. Key: dùng `orderId`.
4. Value:

```json
{
  "eventId": "4ef315dc-f7c9-4c08-9d0b-74a5e49f84fa",
  "eventType": "OrderCreated",
  "occurredAt": "2026-07-10T10:30:00Z",
  "orderId": "a08f5c1d-623c-44c4-a61f-31ce0f305c65",
  "orderCode": "1752143400001",
  "customerId": "53bbcf63-9337-4c68-8f0f-bb277f2ad5dd",
  "totalAmount": 250000,
  "currency": "VND"
}
```

Kỳ vọng:

- Log .NET nhận event.
- DB có payment order mới.
- Nếu payment gateway mock/PayOS config đúng, payment link được attach.
- Consumer group `platform-payment-service` không tăng lag.

### 13.5 Test producer PaymentSucceeded

Gọi webhook PayOS sandbox hoặc dùng endpoint webhook local với raw body hợp lệ.

Kỳ vọng:

- `PaymentOrder` chuyển sang `PAID`.
- Invoice được tạo.
- PDF được generate nếu MinIO config đúng.
- Topic `payment.events` có message `PaymentSucceeded`.

Trong Kafka UI, value phải có dạng:

```json
{
  "eventId": "...",
  "eventType": "PaymentSucceeded",
  "occurredAt": "...",
  "paymentId": "...",
  "orderId": "...",
  "customerId": "...",
  "amount": 250000,
  "paymentMethod": "MOCK",
  "transactionCode": "PAYOS-123",
  "invoiceNumber": null,
  "invoicePdfUrl": null
}
```

---

## 14. Checklist làm code

Làm theo thứ tự này cho ít lỗi nhất:

1. Sửa `appsettings.json`: `localhost:39092`, `GroupId`, topic names.
2. Add package `Confluent.Kafka` vào Infrastructure.
3. Tạo `Infrastructure/Kafka/KafkaSettings.cs`.
4. Tạo event DTO trong `Application/Common/Events`.
5. Tạo `Application/Common/Interfaces/IServices/IEventPublisher.cs`.
6. Tạo `Infrastructure/Kafka/KafkaEventPublisher.cs`.
7. Đăng ký `KafkaSettings` và `IEventPublisher` trong `Infrastructure/DependencyInjection.cs`.
8. Inject `IEventPublisher` vào `ProcessPaymentWebhookCommandHandler`.
9. Sau khi mark paid + tạo invoice, publish `PaymentSucceeded`.
10. Tạo `HandleOrderCreatedCommand` và handler.
11. Tạo `Infrastructure/Kafka/OrderCreatedConsumer.cs`.
12. Đăng ký `services.AddHostedService<OrderCreatedConsumer>()`.
13. Build.
14. Test unit.
15. Chạy Kafka local.
16. Produce thử `OrderCreated`.
17. Xem DB + log .NET.
18. Trigger webhook.
19. Xem `payment.events`.

---

## 15. Các lỗi thường gặp

| Lỗi | Nguyên nhân | Cách xử lý |
|---|---|---|
| Không connect được Kafka | Sai port | App chạy host dùng `localhost:39092`, Docker dùng `kafka:9092` |
| `UNKNOWN_TOPIC_OR_PARTITION` | Topic chưa tạo | Chạy `kafka-init`, kiểm tra `infra/kafka/topics.txt` |
| Consumer nhận message nhưng không xử lý | `eventType` sai hoặc JSON không khớp schema | So message với `contracts/kafka/*.schema.json` |
| Consumer bị retry mãi | Handler throw trước khi commit | Log lỗi, commit message lỗi nghiệp vụ, không commit lỗi hạ tầng |
| PaymentSucceeded thiếu field | DTO không khớp schema | Dùng `PaymentSucceededEvent` trong mục 4 |
| Java không update order | `orderId` publish sai | Tách `PaymentOrder.Id` và `PaymentOrder.OrderId` |
| Event xử lý sai thứ tự | Không set key | Publish với key = `orderId.ToString()` |
| Duplicate tạo payment 2 lần | Không idempotent | Check `orderCode`, `orderId`, hoặc bảng `processed_events` |

---

## 16. Việc nên làm sau MVP

Sau khi flow chạy được, cải thiện theo thứ tự:

1. Thêm bảng `processed_events` để chống xử lý trùng theo `eventId`.
2. Thêm Outbox pattern cho publish `PaymentSucceeded` để tránh DB paid rồi nhưng publish Kafka fail.
3. Thêm retry policy và dead-letter topic cho poison message.
4. Thêm integration test dùng Kafka container hoặc Docker compose.
5. Đồng bộ lại contract nếu PayOS dùng payment method khác `MOCK`, `VNPAY`, `MOMO`.

