using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Invoices.Commands.GenerateInvoicePdf
{
    public class GenerateInvoicePdfCommandHandler : IRequestHandler<GenerateInvoicePdfCommand>
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IInvoicePdfGenerator _pdfGenerator;
        private readonly IFileStorage _fileStorage;
        private readonly ILogger<GenerateInvoicePdfCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateInvoicePdfCommandHandler"/> class.
        /// </summary>
        /// <param name="invoiceRepository">The invoice repository.</param>
        /// <param name="pdfGenerator">The invoice PDF generator.</param>
        /// <param name="fileStorage">The file storage service.</param>
        /// <param name="logger">The logger used to record failures.</param>
        public GenerateInvoicePdfCommandHandler(
            IInvoiceRepository invoiceRepository,
            IInvoicePdfGenerator pdfGenerator,
            IFileStorage fileStorage,
            ILogger<GenerateInvoicePdfCommandHandler> logger)
        {
            _invoiceRepository = invoiceRepository;
            _pdfGenerator = pdfGenerator;
            _fileStorage = fileStorage;
            _logger = logger;
        }

        /// <summary>
        /// Generates and stores a PDF for an invoice when one has not already been attached.
        /// </summary>
        public async Task Handle(GenerateInvoicePdfCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var invoice = await _invoiceRepository.GetByIdAsync(command.InvoiceId, cancellationToken);
                if (invoice is null || invoice.PdfUrl is not null)
                {
                    // Không tồn tại, hoặc PDF bất biến đã sinh rồi (Rule #7)
                    return;
                }

                var pdfBytes = _pdfGenerator.Generate(invoice);
                var url = await _fileStorage.UploadAsync(
                    bucket: "invoices",
                    objectName: $"{invoice.InvoiceNumber}.pdf",
                    content: pdfBytes,
                    contentType: "application/pdf",
                    cancellationToken: cancellationToken);

                invoice.AttachPdf(url);
                await _invoiceRepository.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // PDF fail → invoice vẫn tồn tại với pdf_url = null, retry sau được
                _logger.LogError(ex, "Failed to generate PDF for invoice {InvoiceId}", command.InvoiceId);
            }
        }
    }
}
