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

        public GetInvoicePdfQueryHandler(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

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
