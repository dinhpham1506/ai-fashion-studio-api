using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Invoices.Queries.GetInvoicePdf
{
    public class GetInvoicePdfQueryHandler : IRequestHandler<GetInvoicePdfQuery, InvoicePdfResponse>
    {
        private readonly IInvoiceRepository _invoiceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetInvoicePdfQueryHandler"/> class.
        /// </summary>
        public GetInvoicePdfQueryHandler(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        /// <summary>
        /// Gets the PDF details for an invoice.
        /// </summary>
        /// <param name="request">The invoice PDF query.</param>
        /// <param name="cancellationToken">A token that cancels the operation.</param>
        /// <returns>The invoice number and PDF URL for the requested invoice.</returns>
        /// <exception cref="NotFoundException">Thrown when the invoice does not exist or its PDF is not ready.</exception>
        /// <exception cref="ForbiddenException">Thrown when the requester does not have permission to view the invoice.</exception>
        public async Task<InvoicePdfResponse> Handle(GetInvoicePdfQuery request, CancellationToken cancellationToken)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);
            if (invoice is null)
            {
                throw new NotFoundException("INVOICE_NOT_FOUND", "Invoice not found");
            }

            if (!request.IsStaffOrAdmin && !invoice.BelongTo(request.UserId))
            {
                throw new ForbiddenException("INVOICE_ACCESS_DENIED", "You cannot view this invoice");
            }

            if (invoice.PdfUrl is null)
            {
                throw new NotFoundException("INVOICE_PDF_NOT_READY", "Invoice PDF is not ready yet");
            }

            return new InvoicePdfResponse(invoice.InvoiceNumber, invoice.PdfUrl);
        }
    }
}
