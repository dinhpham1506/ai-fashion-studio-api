using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using AiFashionStudio.Platform.Application.Common.Models;
using Microsoft.Extensions.Options;
using PayOS;
using PayOS.Exceptions;
using PayOS.Models;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebhookVerificationException = AiFashionStudio.Platform.Application.Common.Exceptions.WebhookVerificationException;

namespace AiFashionStudio.Platform.Infrastructure.Payment
{
    public class PayOsPaymentGatewayService : IPaymentGatewayService
    {
        private readonly PayOSClient _payOSClient;
        private readonly PayOsSettings _payOsSettings;

        /// <summary>
        /// Initializes a PayOS payment gateway service.
        /// </summary>
        /// <param name="options">The configured PayOS settings.</param>
        public PayOsPaymentGatewayService(IOptions<PayOsSettings> options)
        {
            _payOsSettings = options.Value;
            _payOSClient = new PayOSClient(_payOsSettings.ClientId, _payOsSettings.ApiKey, _payOsSettings.ChecksumKey);
        }

        /// <summary>
        /// Cancels a payment link for the specified order code.
        /// </summary>
        /// <param name="orderCode">The order code of the payment link to cancel.</param>
        public async Task CancelPaymentLinkAsync(long orderCode, string? reason = null, CancellationToken cancellationToken = default)
        {
            await _payOSClient.PaymentRequests.CancelAsync(
                orderCode,
                "Cancel By Customer!",
                new RequestOptions<CancelPaymentLinkRequest>
                {
                    CancellationToken = cancellationToken
                });
        }

        /// <summary>
        /// Creates a PayOS payment link for the specified payment request.
        /// </summary>
        /// <returns>The created payment link details, including the PayOS payment link ID, checkout URL, and QR code.</returns>
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

        /// <summary>
        /// Gets the current status of a payment link.
        /// </summary>
        /// <param name="orderCode">The order code used to identify the payment link.</param>
        /// <returns>The payment link status name in uppercase.</returns>
        public async Task<string> GetPaymentLinkStatusAsync(long orderCode, CancellationToken cancellationToken = default)
        {
            var link = await _payOSClient.PaymentRequests.GetAsync(orderCode);

            return link.Status.ToString().ToUpperInvariant();
        }

        /// <summary>
        /// Verifies a PayOS webhook payload and maps it to payment data.
        /// </summary>
        /// <param name="rawJsonBody">The raw JSON webhook payload.</param>
        /// <returns>The verified webhook data, including the order code, amount, reference, and payment success status.</returns>
        /// <exception cref="AppUnauthorizedException">Thrown when the payload is invalid or the webhook signature verification fails.</exception>
        public async Task<PaymentWebhookData> VerifyWebhookAsync(string rawJsonBody)
        {
            Webhook? webhook;
            try
            {
                webhook = JsonSerializer.Deserialize<Webhook>(rawJsonBody);
            }
            catch (JsonException)
            {
                throw new WebhookVerificationException("WEBHOOK_SIGNATURE_INVALID", "Webhook payload is invalid");
            }

            if (webhook is null)
            {
                throw new WebhookVerificationException("WEBHOOK_SIGNATURE_INVALID", "Webhook payload is invalid");
            }

            try
            {
                // SDK tự tính HMAC-SHA256 bằng ChecksumKey và so với field signature — sai thì throw
                var data = await _payOSClient.Webhooks.VerifyAsync(webhook);
                return new PaymentWebhookData(data.OrderCode, data.Amount, data.Reference, data.Code == "00");
            }
            catch (InvalidSignatureException)
            {
                throw new WebhookVerificationException("WEBHOOK_SIGNATURE_INVALID", "Webhook signature verification failed");
            }
        }
    }
}
