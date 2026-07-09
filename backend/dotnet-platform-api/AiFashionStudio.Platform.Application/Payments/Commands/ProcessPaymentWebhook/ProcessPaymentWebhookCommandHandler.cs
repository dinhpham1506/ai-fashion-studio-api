using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using AiFashionStudio.Platform.Application.Invoices.Commands.CreateInvoice;
using AiFashionStudio.Platform.Application.Invoices.Commands.GenerateInvoicePdf;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Payments.Commands.ProcessPaymentWebhook
{
    public class ProcessPaymentWebhookCommandHandler : IRequestHandler<ProcessPaymentWebhookCommand>
    {
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly IPaymentOrderRepository _paymentOrderRepository;
        private readonly ISender _sender;
        private ILogger<ProcessPaymentWebhookCommandHandler> _logger;

        public ProcessPaymentWebhookCommandHandler(IPaymentGatewayService paymentGatewayService,
            IPaymentOrderRepository paymentOrderRepository,
            ISender sender,
            ILogger<ProcessPaymentWebhookCommandHandler> logger)
        {
            _paymentGatewayService = paymentGatewayService;
            _paymentOrderRepository = paymentOrderRepository;
            _sender = sender;
            _logger = logger;
        }
        public async Task Handle(ProcessPaymentWebhookCommand command, CancellationToken cancellationToken)
        {
            var webhook = await _paymentGatewayService.VerifyWebhookAsync(command.RawBody);

            var order = await _paymentOrderRepository.GetByOrderCodeAsync(webhook.OrderCode,cancellationToken);
            if (order == null)
            {
                _logger.LogInformation($"Webhook for unknown orderCode {webhook.OrderCode} ignored");

                return;
            }

            if (!webhook.IsSuccess)
            {
                _logger.LogInformation($"Webhook for orderCode {webhook.OrderCode} reported failure, ignored");
                return;
            }

            if (order.Amount != webhook.Amount)
            {
                _logger.LogWarning(
                    $"Webhook amount mismatch for orderCode {webhook.OrderCode}: expected {order.Amount}, got {webhook.Amount}" );

                return; // KHÔNG đánh dấu PAID khi lệch tiền
            }

            order.MarkPaid(webhook.Reference); //Idempotency

            await _paymentOrderRepository.SaveChangesAsync(cancellationToken);

            // Invoice hook: sau PAID, không throw, không ảnh hưởng response webhook —
            // CreateInvoiceCommandHandler tự bắt lỗi và trả null nếu tạo invoice thất bại
            var invoiceId = await _sender.Send(
                new CreateInvoiceCommand(order.Id, order.Id, order.UserId, order.Description, order.Amount),
                cancellationToken);

            if (invoiceId is not null)
            {
                await _sender.Send(new GenerateInvoicePdfCommand(invoiceId.Value), cancellationToken);
            }
        }
    }
}
