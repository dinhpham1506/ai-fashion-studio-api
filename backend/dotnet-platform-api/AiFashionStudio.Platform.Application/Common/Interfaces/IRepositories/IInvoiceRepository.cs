using AiFashionStudio.Platform.Domain.Invoice.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories
{
    public interface IInvoiceRepository : IBaseRepository<Invoice>
    {
        /// <summary>
/// Gets the invoice associated with an order.
/// </summary>
/// <param name="orderId">The order identifier.</param>
/// <returns>The invoice for the specified order, or <c>null</c> if no invoice exists.</returns>
        Task<Invoice?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

        /// <summary>
/// Determines whether an invoice exists for the specified order.
/// </summary>
/// <param name="orderId">The order identifier to check.</param>
/// <param name="cancellationToken">A token used to cancel the operation.</param>
/// <returns><c>true</c> if an invoice exists for the specified order; otherwise, <c>false</c>.</returns>
        Task<bool> ExistsForOrderAsync(Guid orderId, CancellationToken cancellationToken = default);

        /// <summary>
/// Counts the invoices issued on a specific date.
/// </summary>
/// <param name="issuedDate">The date to count issued invoices for.</param>
/// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
/// <returns>The number of invoices issued on the specified date.</returns>
        Task<int> CountIssuedTodayAsync(DateOnly issuedDate, CancellationToken cancellationToken = default);

        Task<Invoice> IssueForOrderAsync(
            Guid orderId,
            Guid paymentId,
            Guid customerId,
            string description,
            decimal amount,
            CancellationToken cancellationToken = default);
    }
}
