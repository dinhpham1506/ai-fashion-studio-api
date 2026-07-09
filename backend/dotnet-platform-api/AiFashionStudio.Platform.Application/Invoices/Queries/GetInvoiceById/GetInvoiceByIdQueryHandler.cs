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

        public GetInvoiceByIdQueryHandler(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

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
