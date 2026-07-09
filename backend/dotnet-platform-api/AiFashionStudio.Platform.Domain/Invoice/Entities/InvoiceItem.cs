using AiFashionStudio.Platform.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Domain.Invoice.Entities
{
    public class InvoiceItem : BaseEntity
    {
        public string ProductNameSnapshot { get; private set; } = default!;
        public string? VariantSnapshot { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal TotalPrice => UnitPrice * Quantity;

        private InvoiceItem()
        {
        }

        public static InvoiceItem Create(string productNameSnapshot, string? variantSnapshot, int quantity, decimal unitPrice)
        {
            return new InvoiceItem
            {
                ProductNameSnapshot = productNameSnapshot,
                VariantSnapshot = variantSnapshot,
                Quantity = quantity,
                UnitPrice = unitPrice
            };
        }
    }
}
