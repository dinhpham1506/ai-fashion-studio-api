using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Invoices.Queries.GetInvoiceItems
{
    public class GetInvoiceItemsQueryHandler : IRequestHandler<GetInvoiceItemsQuery, InvoiceItemsResponse>
    {
        private readonly IInvoiceRepository _invoiceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetInvoiceItemsQueryHandler"/> class.
        /// </summary>
        /// <param name="invoiceRepository">The invoice repository used to load invoice data.</param>
        public GetInvoiceItemsQueryHandler(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        /// <summary>
        /// Gets the items for an invoice.
        /// </summary>
        /// <param name="request">The query containing the invoice ID and caller access information.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The invoice items response.</returns>
        /// <exception cref="NotFoundException">Thrown when the invoice cannot be found.</exception>
        /// <exception cref="ForbiddenException">Thrown when the caller is not allowed to view the invoice.</exception>
        public async Task<InvoiceItemsResponse> Handle(GetInvoiceItemsQuery request, CancellationToken cancellationToken)
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

            var items = invoice.Items
                .Select(item => new InvoiceItemResponse(
                    item.ProductNameSnapshot, item.VariantSnapshot,
                    item.Quantity, item.UnitPrice, item.TotalPrice))
                .ToList();

            return new InvoiceItemsResponse(items);
        }
    }
}
