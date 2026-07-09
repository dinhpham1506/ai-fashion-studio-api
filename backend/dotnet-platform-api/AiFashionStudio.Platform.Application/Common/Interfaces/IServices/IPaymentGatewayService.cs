using AiFashionStudio.Platform.Application.Common.Models;

namespace AiFashionStudio.Platform.Application.Common.Interfaces.IServices
{
    public interface IPaymentGatewayService
    {
        Task<PaymentLinkResponse> CreatePaymentLinkAsync(PaymentLinkRequest request, CancellationToken cancellationToken = default);

        // Trả status PayOS đang giữ
        Task<string> GetPaymentLinkStatusAsync(long orderCode, CancellationToken cancellationToken = default);

        Task CancelPaymentLinkAsync(long orderCode, string? reason = null, CancellationToken cancellationToken = default);

        // Parse + verify chữ ký raw body webhook. 
        Task<PaymentWebhookData> VerifyWebhookAsync(string rawJsonBody);
    }
}
