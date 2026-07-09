using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Domain.Invoice.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Repositories
{
    public class InvoiceRepository : BaseRepository<Invoice>, IInvoiceRepository
    {
        private readonly AppDbContext _appDbContext;

        /// <summary>
        /// Initializes a new invoice repository.
        /// </summary>
        public InvoiceRepository(AppDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        /// <summary>
                /// Gets an invoice by its identifier and includes its items.
                /// </summary>
                /// <param name="id">The invoice identifier.</param>
                /// <param name="cancellationToken">A token used to cancel the operation.</param>
                /// <returns>The matching invoice, or <c>null</c> if no invoice is found.</returns>
        public override Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _appDbContext.Invoices.Include(invoice => invoice.Items)
                .FirstOrDefaultAsync(invoice => invoice.Id == id, cancellationToken);

        /// <summary>
                /// Gets the invoice for the specified order.
                /// </summary>
                /// <param name="orderId">The order identifier to match.</param>
                /// <param name="cancellationToken">A token used to cancel the operation.</param>
                /// <returns>The matching invoice, or null if no invoice exists for the order.</returns>
                public Task<Invoice?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
            => _appDbContext.Invoices.Include(invoice => invoice.Items)
                .FirstOrDefaultAsync(invoice => invoice.OrderId == orderId, cancellationToken);

        /// <summary>
            /// Determines whether an invoice exists for the specified order.
            /// </summary>
            /// <returns>
            /// <c>true</c> if at least one invoice is associated with the order; otherwise, <c>false</c>.
            /// </returns>
            public Task<bool> ExistsForOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
            => _appDbContext.Invoices.AnyAsync(invoice => invoice.OrderId == orderId, cancellationToken);

        /// <summary>
        /// Counts invoices issued on the specified date.
        /// </summary>
        /// <param name="issuedDate">The date to count invoices for.</param>
        /// <returns>The number of invoices issued on the specified date.</returns>
        public Task<int> CountIssuedTodayAsync(DateOnly issuedDate, CancellationToken cancellationToken = default)
        {
            var startOfDay = issuedDate.ToDateTime(TimeOnly.MinValue);
            var startOfNextDay = issuedDate.AddDays(1).ToDateTime(TimeOnly.MinValue);

            return _appDbContext.Invoices.CountAsync(
                invoice => invoice.IssuedAt >= startOfDay && invoice.IssuedAt < startOfNextDay,
                cancellationToken);
        }
    }
}
