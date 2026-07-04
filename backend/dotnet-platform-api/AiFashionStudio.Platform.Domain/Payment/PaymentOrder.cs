namespace AiFashionStudio.Platform.Domain.Payment;

/// <summary>
/// Pure domain model for an accepted payment order.
/// </summary>
public sealed record PaymentOrder
{
    public Guid Id { get; }

    public decimal Amount { get; }

    public string Currency { get; }

    private PaymentOrder(Guid id, decimal amount, string currency)
    {
        Id = id;
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Factory that creates a new payment order with a generated identity.
    /// </summary>
    public static PaymentOrder Create(decimal amount, string currency)
        => new(Guid.NewGuid(), amount, currency);
}
