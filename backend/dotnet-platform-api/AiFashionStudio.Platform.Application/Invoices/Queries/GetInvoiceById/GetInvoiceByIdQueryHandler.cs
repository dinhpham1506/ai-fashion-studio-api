using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Invoices.Queries.GetInvoiceById
{
    public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceResponse>
    {
        private readonly IInvoiceRepository _invoiceRepository;

        /// <summary>
        /// Creates a new instance of the handler.
        /// </summary>
        /// <param name="invoiceRepository">The invoice repository used to load invoice data.</param>
        public GetInvoiceByIdQueryHandler(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        /// <summary>
        /// Gets an invoice by ID and verifies the caller can view it.
        /// </summary>
        /// <param name="request">The invoice lookup details and access context.</param>
        /// <returns>The invoice response for the requested invoice.</returns>
        /// <exception cref="NotFoundException">Thrown when the invoice cannot be found.</exception>
        /// <exception cref="ForbiddenException">Thrown when the caller is not allowed to view the invoice.</exception>
        public async Task<InvoiceResponse> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);
            if (invoice is null)
            {
                throw new NotFoundException("INVOICE_NOT_FOUND", "Invoice not found");
            }

            // INV-BR-005: chỉ chủ đơn hoặc STAFF/ADMIN
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
