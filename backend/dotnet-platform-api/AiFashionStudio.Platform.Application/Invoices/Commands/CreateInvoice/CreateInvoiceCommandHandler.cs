using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Domain.Invoice.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Invoices.Commands.CreateInvoice
{
    public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, Guid?>
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ILogger<CreateInvoiceCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateInvoiceCommandHandler"/> class.
        /// </summary>
        public CreateInvoiceCommandHandler(IInvoiceRepository invoiceRepository, ILogger<CreateInvoiceCommandHandler> logger)
        {
            _invoiceRepository = invoiceRepository;
            _logger = logger;
        }

        /// <summary>
        /// Creates an invoice for the specified order.
        /// </summary>
        /// <param name="command">The invoice creation request.</param>
        /// <returns>The created invoice identifier, or null if an invoice already exists for the order or creation fails.</returns>
        public async Task<Guid?> Handle(CreateInvoiceCommand command, CancellationToken cancellationToken)
        {
            try
            {   
                // ngăn chặn để tạo invoice thứ 2
                if( await _invoiceRepository.ExistsForOrderAsync(command.OrderId, cancellationToken))
                {
                    return null;
                }

                var invoiceNumber = await GenerateInvoiceNumberAsync(cancellationToken);

                //MVP: chưa có Java Order API
                var items = new[]
                {
                    InvoiceItem.Create(command.Description,variantSnapshot: null, quantity:1, unitPrice: command.Amount)
                };

                var invoice = Invoice.Issue(
                    orderId: command.OrderId, PaymentId: command.PaymentId,
                    CustomerId: command.CustomerId,
                    invoiceNumber, "VND", items);

                await _invoiceRepository.AddAsync(invoice, cancellationToken);
                await _invoiceRepository.SaveChangesAsync(cancellationToken);
                return invoice.Id;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Failed to create invoice for order {OrderId}", command.OrderId);
                return null;
            }

        }
        /// <summary>
        /// Generates the next invoice number for the current UTC date.
        /// </summary>
        /// <returns>The invoice number in <c>INVyyyMMdd####</c> format.</returns>
        private async Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var seq = await _invoiceRepository.CountIssuedTodayAsync(today,cancellationToken) + 1;

            // trả INV + yyyMMdd + 4 số 
            return $"INV{today:yyyMMdd}{seq:D4}";
        }
    }
}
