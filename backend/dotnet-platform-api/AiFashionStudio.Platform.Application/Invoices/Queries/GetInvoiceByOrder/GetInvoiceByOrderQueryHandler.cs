using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Invoices.Queries.GetInvoiceByOrder
{
    public class GetInvoiceByOrderQueryHandler : IRequestHandler<GetInvoiceByOrderQuery, InvoiceResponse>
    {
        private readonly IInvoiceRepository _invoiceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetInvoiceByOrderQueryHandler"/> class.
        /// </summary>
        /// <param name="invoiceRepository">The repository used to retrieve invoice data.</param>
        public GetInvoiceByOrderQueryHandler(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        /// <summary>
        /// Retrieves an invoice for the specified order.
        /// </summary>
        /// <param name="request">The query containing the order identifier and caller access details.</param>
        /// <param name="cancellationToken">A token to observe while processing the request.</param>
        /// <returns>The invoice response for the matching order.</returns>
        /// <exception cref="NotFoundException">Thrown when no invoice exists for the specified order.</exception>
        /// <exception cref="ForbiddenException">Thrown when the caller is not allowed to view the invoice.</exception>
        public async Task<InvoiceResponse> Handle(GetInvoiceByOrderQuery request, CancellationToken cancellationToken)
        {
            var invoice = await _invoiceRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
            if (invoice is null)
            {
                throw new NotFoundException("INVOICE_NOT_FOUND", "Invoice not found");
            }

            if (!request.IsStaffOrAdmin && !invoice.BelongTo(request.UserId))
            {
                throw new ForbiddenException("INVOICE_ACCESS_DENIED", "You cannot view this invoice");
            }

            return new InvoiceResponse(
                invoice.Id, invoice.OrderId, invoice.InvoiceNumber,
                invoice.TotalAmount, invoice.Currency,
                invoice.Status.ToString().ToUpperInvariant(),
                invoice.PdfUrl, invoice.IssuedAt ?? invoice.CreatedAt);
        }
    }
}
