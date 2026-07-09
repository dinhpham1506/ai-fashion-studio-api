using AiFashionStudio.Platform.Application.Common.Models;

namespace AiFashionStudio.Platform.Application.Common.Interfaces.IServices
{
    public interface IPaymentGatewayService
    {
        /// <summary>
/// Creates a payment link for the specified request.
/// </summary>
/// <param name="request">The payment link details to create.</param>
/// <returns>The created payment link response.</returns>
Task<PaymentLinkResponse> CreatePaymentLinkAsync(PaymentLinkRequest request, CancellationToken cancellationToken = default);

        /// <summary>
/// Gets the current status of a payment link for the specified order code.
/// </summary>
/// <param name="orderCode">The order code associated with the payment link.</param>
/// <param name="cancellationToken">A token used to cancel the operation.</param>
/// <returns>The payment link status.</returns>
        Task<string> GetPaymentLinkStatusAsync(long orderCode, CancellationToken cancellationToken = default);

        /// <summary>
/// Cancels a payment link for the specified order code.
/// </summary>
/// <param name="orderCode">The order code of the payment link to cancel.</param>
/// <param name="reason">The optional cancellation reason.</param>
Task CancelPaymentLinkAsync(long orderCode, string? reason = null, CancellationToken cancellationToken = default);

        /// <summary>
/// Verifies an incoming webhook payload and parses its contents.
/// </summary>
/// <param name="rawJsonBody">The raw JSON body received from the webhook request.</param>
/// <returns>The verified webhook data parsed from the payload.</returns>
        Task<PaymentWebhookData> VerifyWebhookAsync(string rawJsonBody);
    }
}
