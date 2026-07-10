using AiFashionStudio.Platform.Domain.Common;
using AiFashionStudio.Platform.Domain.Invoice.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Domain.Invoice.Entities
{
    public class Invoice : BaseEntity
    {
        public Guid OrderId { get; private set; }
        public Guid PaymentId { get; private set; }
        public Guid CustomerId { get; private set; }
        public string InvoiceNumber { get; private set; } = default!;
        public decimal TotalAmount { get; private set; }
        public string Currency { get; private set; } = "VND";
        public InvoiceStatus Status { get; private set; } = InvoiceStatus.Issued;
        public string? PdfUrl { get; private set; }
        public DateTime? IssuedAt { get; private set; }

        private readonly List<InvoiceItem> _items = new();
        public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();

        private Invoice()
        {

        }

        /// <summary>
        /// Creates an issued invoice from the supplied order, payment, customer, and items.
        /// </summary>
        /// <param name="orderId">The associated order identifier.</param>
        /// <param name="PaymentId">The associated payment identifier.</param>
        /// <param name="CustomerId">The customer identifier.</param>
        /// <param name="invoiceNumber">The invoice number.</param>
        /// <param name="currency">The invoice currency code.</param>
        /// <param name="items">The invoice items to include.</param>
        /// <returns>The created invoice.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no invoice items are provided.</exception>
        public static Invoice Issue(Guid orderId, Guid PaymentId, Guid CustomerId, string invoiceNumber, string currency, IEnumerable<InvoiceItem> items)
        {
            var invoice = new Invoice()
            {
                OrderId = orderId,
                PaymentId = PaymentId,
                CustomerId = CustomerId,
                InvoiceNumber = invoiceNumber,
                Currency = currency,
                IssuedAt = DateTime.UtcNow
            };

            invoice._items.AddRange(items);

            if (invoice._items.Count == 0)
            {
                throw new InvalidOperationException("Invoice must have at least one item.");
            }

            invoice.TotalAmount = invoice._items.Sum(i => i.TotalPrice);
            return invoice;
        }

        /// <summary>
        /// Attaches a PDF URL to the invoice.
        /// </summary>
        /// <param name="pdfUrl">The PDF URL to store.</param>
        public void AttachPdf(string pdfUrl)
        {
            // PDF bất biến — chỉ gắn khi chưa có
            if (PdfUrl is null)
            {
                PdfUrl = pdfUrl;
            }
        }
        
        /// <summary>
/// Determines whether the invoice belongs to a customer.
/// </summary>
/// <param name="UserId">The user ID to compare with the invoice customer ID.</param>
/// <returns><c>true</c> if the invoice customer ID matches the specified user ID, <c>false</c> otherwise.</returns>
public bool BelongTo(Guid UserId) => CustomerId == UserId;





    }
}
