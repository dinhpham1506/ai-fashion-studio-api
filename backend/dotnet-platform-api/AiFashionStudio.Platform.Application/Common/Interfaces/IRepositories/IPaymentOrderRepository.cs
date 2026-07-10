using AiFashionStudio.Platform.Domain.Payment.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories
{
    public interface IPaymentOrderRepository : IBaseRepository<PaymentOrder>
    {

        /// <summary>
/// Retrieves a payment order by its order code.
/// </summary>
/// <param name="orderCode">The order code of the payment order to retrieve.</param>
/// <returns>The payment order with the specified order code, or <c>null</c> if no match is found.</returns>
Task<PaymentOrder?> GetByOrderCodeAsync(long orderCode, CancellationToken cancellationToken = default);

        /// <summary>
/// Retrieves a payment order by order code and user.
/// </summary>
/// <param name="orderCode">The order code.</param>
/// <param name="userId">The user identifier.</param>
/// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
/// <returns>The matching payment order, or <c>null</c> if no order exists for the specified code and user.</returns>
        Task<PaymentOrder?> GetByOrderCodeAndUserIdAsync(long orderCode, Guid userId, CancellationToken cancellationToken = default);
    }
}
