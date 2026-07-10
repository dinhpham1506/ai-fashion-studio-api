using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Invoices.Queries.GetInvoicePdf
{
    public class GetInvoicePdfQueryHandler : IRequestHandler<GetInvoicePdfQuery, InvoicePdfResponse>
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IFileStorage _fileStorage;

        public GetInvoicePdfQueryHandler(IInvoiceRepository invoiceRepository, IFileStorage fileStorage)
        {
            _invoiceRepository = invoiceRepository;
            _fileStorage = fileStorage;
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

            var pdfUrl = await _fileStorage.GetTemporaryUrlAsync(
                bucket: "invoices",
                objectName: invoice.PdfUrl,
                expiresIn: TimeSpan.FromMinutes(5),
                cancellationToken: cancellationToken);

            return new InvoicePdfResponse(invoice.InvoiceNumber, pdfUrl);
        }
    }
}
