using AiFashionStudio.Platform.Domain.Common;
using AiFashionStudio.Platform.Domain.Payment.Enums;

namespace AiFashionStudio.Platform.Domain.Payment.Entities;

/// <summary>
/// Pure domain model for an accepted payment order.
/// </summary>
public class PaymentOrder : UpdatableEntity
{
    public Guid UserId { get; private set; }
    public long OrderCode { get; private set; }
    public int Amount { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? PaymentLinkId { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public string? GatewayReference { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    private PaymentOrder() { }

    public static PaymentOrder Create(Guid UserId, long OrderCode, int amount, string description)
    {
        return new() 
        { 
            UserId = UserId,
            OrderCode = OrderCode,
            Amount = amount,
            Description = description
        };

    }

    // Kiểm tra đã thanh toán chưa
    public bool IsPending() => Status == PaymentStatus.Pending;

    //Gắn link sau khi tạo PayOS
    public void  AttachPaymentLink(string paymentLinkId)
    {
        PaymentLinkId = paymentLinkId;
        Update();
    }

    //Idempotency: ngăn gọi lại khi đã trả thanh toán rồi
    public void MarkPaid(string gatewayReference) 
    {
        if ( Status == PaymentStatus.Paid)
        {
            return;
        }

        Status = PaymentStatus.Paid;
        GatewayReference = gatewayReference;
        PaidAt = DateTime.UtcNow;
        Update();
    }

    // cập nhật khi người dùng hủy thanh toán
    public void Cancel()
    {
        Status = PaymentStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        Update();
    }

    // cập nhật khi thanh toán hết hạn
    public void MarkExpired()
    {
        Status = PaymentStatus.Expired;
        Update();
    }

}
