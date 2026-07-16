namespace AiFashionStudio.Platform.Application.Common.Models;

/// <summary>
/// Envelope chuẩn cho mọi Kafka event giữa C# và Java (theo Backend API Spec mục 16).
/// </summary>
public sealed record IntegrationEventEnvelope<T>(
    Guid EventId,
    string EventType,
    DateTime OccurredAt,
    T Data)
{
    public static IntegrationEventEnvelope<T> Create(string eventType, T data) =>
        new(Guid.NewGuid(), eventType, DateTime.UtcNow, data);
}

/// <summary>Payload event OrderCreated do Java Order Service publish lên topic order.events.</summary>
public sealed record OrderCreatedEventData(
    Guid OrderId,
    string OrderCode,
    Guid CustomerId,
    decimal TotalAmount);

/// <summary>Payload event PaymentSucceeded — Java consume để chuyển order sang PAID và lock design.</summary>
public sealed record PaymentSucceededEventData(
    Guid PaymentId,
    Guid? OrderId,
    long OrderCode,
    Guid CustomerId,
    decimal Amount,
    string Provider,
    string? TransactionCode,
    DateTime PaidAt);

/// <summary>Payload event PaymentFailed — Java ghi nhận thanh toán thất bại, order giữ PENDING_PAYMENT.</summary>
public sealed record PaymentFailedEventData(
    Guid? PaymentId,
    Guid? OrderId,
    long OrderCode,
    string Provider,
    string Reason);
