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

    /// <summary>
    /// Creates a payment order with the provided core details.
    /// </summary>
    /// <param name="UserId">The identifier of the user.</param>
    /// <param name="OrderCode">The order code.</param>
    /// <param name="amount">The payment amount.</param>
    /// <param name="description">The order description.</param>
    /// <returns>The created payment order.</returns>
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

    /// <summary>
/// Determines whether the payment order is still pending.
/// </summary>
/// <returns>
/// <c>true</c> if the order status is <see cref="PaymentStatus.Pending"/>; otherwise, <c>false</c>.
/// </returns>
    public bool IsPending() => Status == PaymentStatus.Pending;

    public bool IsCancellationRequested() => Status == PaymentStatus.CancelRequested;

    //Gắn link sau khi tạo PayOS
    public void  AttachPaymentLink(string paymentLinkId)
    {
        PaymentLinkId = paymentLinkId;
        Update();
    }

    /// <summary>
    /// Marks the order as paid.
    /// </summary>
    /// <param name="gatewayReference">The reference provided by the payment gateway.</param>
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
    public void MarkCancellationRequested()
    {
        if (Status != PaymentStatus.Pending)
        {
            return;
        }

        Status = PaymentStatus.CancelRequested;
        Update();
    }

    public void Cancel()
    {
        Status = PaymentStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        Update();
    }

    /// <summary>
    /// Marks the payment order as expired.
    /// </summary>
    public void MarkExpired()
    {
        Status = PaymentStatus.Expired;
        Update();
    }

}
