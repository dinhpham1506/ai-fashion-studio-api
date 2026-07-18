using AiFashionStudio.Platform.Application.Common.Models;

namespace AiFashionStudio.Platform.Application.Common.Interfaces.IServices;

/// <summary>
/// Publish payment lifecycle events lên Kafka topic payment.events.
/// C# owns payment — Java Order Service consume các event này để cập nhật order
/// (C# không update trực tiếp bảng orders theo service boundary rule).
/// </summary>
public interface IPaymentEventPublisher
{
    /// <summary>Báo payment thành công — Java chuyển order sang PAID và lock design.</summary>
    Task PublishPaymentSucceededAsync(PaymentSucceededEventData data, CancellationToken cancellationToken);

    /// <summary>Báo payment thất bại — order bên Java giữ nguyên PENDING_PAYMENT.</summary>
    Task PublishPaymentFailedAsync(PaymentFailedEventData data, CancellationToken cancellationToken);
}
