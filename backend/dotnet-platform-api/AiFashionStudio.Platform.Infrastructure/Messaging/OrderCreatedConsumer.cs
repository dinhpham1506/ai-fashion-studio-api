using System.Text.Json;
using AiFashionStudio.Platform.Application.Common.Models;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiFashionStudio.Platform.Infrastructure.Messaging;

/// <summary>
/// Consume topic order.events từ Java Order Service.
/// OrderCreated báo có đơn mới cần thanh toán — hiện tại ghi nhận/log để
/// FE chủ động gọi POST /api/payments (REST) theo sequence diagram;
/// đây là điểm mở rộng nếu sau này muốn auto-create payment record.
/// </summary>
public sealed class OrderCreatedConsumer : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly KafkaSettings _settings;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(IOptions<KafkaSettings> options, ILogger<OrderCreatedConsumer> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled || string.IsNullOrWhiteSpace(_settings.BootstrapServers))
        {
            _logger.LogWarning("Kafka disabled — OrderCreatedConsumer not started");
            return Task.CompletedTask;
        }

        // Consume loop chạy trên thread riêng vì Confluent Consume() là blocking call
        return Task.Factory.StartNew(() => ConsumeLoop(stoppingToken), stoppingToken,
            TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private void ConsumeLoop(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var consumer = new ConsumerBuilder<string, string>(config).Build();
                consumer.Subscribe(_settings.OrderEventsTopic);
                _logger.LogInformation("OrderCreatedConsumer subscribed to {Topic}", _settings.OrderEventsTopic);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = consumer.Consume(stoppingToken);
                    if (result?.Message?.Value is null)
                    {
                        continue;
                    }

                    HandleMessage(result.Message.Value);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                // Broker down / lỗi tạm thời — chờ rồi kết nối lại, không làm sập app
                _logger.LogError(exception, "OrderCreatedConsumer error — retrying in 10s");
                if (stoppingToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(10)))
                {
                    break;
                }
            }
        }
    }

    private void HandleMessage(string payload)
    {
        try
        {
            var envelope = JsonSerializer.Deserialize<IntegrationEventEnvelope<OrderCreatedEventData>>(payload, JsonOptions);
            if (envelope is null || !string.Equals(envelope.EventType, "OrderCreated", StringComparison.OrdinalIgnoreCase))
            {
                // Topic order.events còn chứa OrderCompleted... — chỉ xử lý OrderCreated ở consumer này
                return;
            }

            _logger.LogInformation(
                "OrderCreated received: orderId {OrderId}, orderCode {OrderCode}, customer {CustomerId}, amount {Amount}",
                envelope.Data.OrderId, envelope.Data.OrderCode, envelope.Data.CustomerId, envelope.Data.TotalAmount);
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(exception, "Skipped malformed message on {Topic}", _settings.OrderEventsTopic);
        }
    }
}
