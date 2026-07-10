using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Invoices.Commands.GenerateInvoicePdf
{
    public class GenerateInvoicePdfCommandHandler : IRequestHandler<GenerateInvoicePdfCommand>
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IInvoicePdfGenerator _pdfGenerator;
        private readonly IFileStorage _fileStorage;

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
            IFileStorage fileStorage)
        {
            _invoiceRepository = invoiceRepository;
            _pdfGenerator = pdfGenerator;
            _fileStorage = fileStorage;
        }

        /// <summary>
        /// Generates and stores a PDF for an invoice when one has not already been attached.
        /// </summary>
        public async Task Handle(GenerateInvoicePdfCommand command, CancellationToken cancellationToken)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(command.InvoiceId, cancellationToken);
            if (invoice is null || invoice.PdfUrl is not null)
            {
                return;
            }

            var objectName = $"{invoice.InvoiceNumber}.pdf";
            var pdfBytes = _pdfGenerator.Generate(invoice);
            await _fileStorage.UploadAsync(
                bucket: "invoices",
                objectName: objectName,
                content: pdfBytes,
                contentType: "application/pdf",
                cancellationToken: cancellationToken);

            invoice.AttachPdf(objectName);
            await _invoiceRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
