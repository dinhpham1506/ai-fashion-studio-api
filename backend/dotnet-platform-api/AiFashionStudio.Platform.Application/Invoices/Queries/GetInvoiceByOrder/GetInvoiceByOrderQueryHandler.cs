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

        public GetInvoiceByOrderQueryHandler(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

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
