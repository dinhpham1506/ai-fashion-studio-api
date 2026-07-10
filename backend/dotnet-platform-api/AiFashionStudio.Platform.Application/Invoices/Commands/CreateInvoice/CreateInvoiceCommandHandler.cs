using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using MediatR;
using System;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Invoices.Commands.CreateInvoice
{
    public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, Guid?>
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public CreateInvoiceCommandHandler(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        /// <summary>
        /// Creates an invoice for the specified order.
        /// </summary>
        /// <param name="command">The invoice creation request.</param>
        /// <returns>The created invoice identifier, or null if an invoice already exists for the order or creation fails.</returns>
        public async Task<Guid?> Handle(CreateInvoiceCommand command, CancellationToken cancellationToken)
        {
            var invoice = await _invoiceRepository.IssueForOrderAsync(
                command.OrderId,
                command.PaymentId,
                command.CustomerId,
                command.Description,
                command.Amount,
                cancellationToken);

            return invoice.Id;
        }
    }
}
