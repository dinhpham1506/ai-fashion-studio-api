using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using AiFashionStudio.Platform.Application.Common.Models;
using Microsoft.Extensions.Options;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AppUnauthorizedException = AiFashionStudio.Platform.Application.Common.Exceptions.UnauthorizedException;

namespace AiFashionStudio.Platform.Infrastructure.Payment
{
    public class PayOsPaymentGatewayService : IPaymentGatewayService
    {
        private readonly PayOSClient _payOSClient;
        private readonly PayOsSettings _payOsSettings;

        public PayOsPaymentGatewayService(IOptions<PayOsSettings> options)
        {
            _payOsSettings = options.Value;
            _payOSClient = new PayOSClient(_payOsSettings.ClientId, _payOsSettings.ApiKey, _payOsSettings.ChecksumKey);
        }

        public async Task CancelPaymentLinkAsync(long orderCode, string? reason = null, CancellationToken cancellationToken = default)
        {
             await _payOSClient.PaymentRequests.CancelAsync(orderCode);
        }

        public async Task<PaymentLinkResponse> CreatePaymentLinkAsync(PaymentLinkRequest request, CancellationToken cancellationToken = default)
        {
            var paymentData = new CreatePaymentLinkRequest
            {
                OrderCode = request.OrderCode,
                Amount = request.Amount,
                Description = request.Description,
                ReturnUrl = string.IsNullOrEmpty(request.ReturnUrl) ? _payOsSettings.ReturnUrl : request.ReturnUrl,
                CancelUrl = string.IsNullOrEmpty(request.CancelUrl) ? _payOsSettings.CancelUrl : request.CancelUrl
            };

            var result = await _payOSClient.PaymentRequests.CreateAsync(paymentData);

            return new PaymentLinkResponse(result.PaymentLinkId, result.CheckoutUrl, result.QrCode);

        }

        public async Task<string> GetPaymentLinkStatusAsync(long orderCode, CancellationToken cancellationToken = default)
        {
            var link = await _payOSClient.PaymentRequests.GetAsync(orderCode);

            return link.Status.ToString().ToUpperInvariant();
        }

        public async Task<PaymentWebhookData> VerifyWebhookAsync(string rawJsonBody)
        {
            Webhook? webhook;
            try
            {
                webhook = JsonSerializer.Deserialize<Webhook>(rawJsonBody);
            }
            catch (JsonException)
            {
                throw new AppUnauthorizedException("WEBHOOK_SIGNATURE_INVALID", "Webhook payload is invalid");
            }

            if (webhook is null)
            {
                throw new AppUnauthorizedException("WEBHOOK_SIGNATURE_INVALID", "Webhook payload is invalid");
            }

            try
            {
                // SDK tự tính HMAC-SHA256 bằng ChecksumKey và so với field signature — sai thì throw
                var data = await _payOSClient.Webhooks.VerifyAsync(webhook);
                return new PaymentWebhookData(data.OrderCode, data.Amount, data.Reference, data.Code == "00");
            }
            catch (Exception)
            {
                // InvalidSignatureException / WebhookException của SDK → quy về 401 của app
                throw new AppUnauthorizedException("WEBHOOK_SIGNATURE_INVALID", "Webhook signature verification failed");
            }
        }
    }
}
