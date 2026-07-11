using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Domain.Payment.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Repositories
{
    public class PaymentOrderRepository : BaseRepository<PaymentOrder>, IPaymentOrderRepository
    {
        private readonly AppDbContext _appDbContext;

        /// <summary>
        /// Creates a new payment order repository.
        /// </summary>
        /// <param name="appDbContext">The database context used by the repository.</param>
        public PaymentOrderRepository(AppDbContext appDbContext) : base(appDbContext)
        {
            
                _appDbContext = appDbContext;
            
        }

        /// <summary>
        /// Gets the payment order for the specified order code and user.
        /// </summary>
        /// <param name="orderCode">The order code to match.</param>
        /// <param name="userId">The user identifier to match.</param>
        /// <param name="cancellationToken">A token used to cancel the operation.</param>
        /// <returns>The matching payment order, or <c>null</c> if no match is found.</returns>
        public async Task<PaymentOrder?> GetByOrderCodeAndUserIdAsync(long orderCode, Guid userId, CancellationToken cancellationToken = default)
        {
            //Tìm đơn thanh toán theo mã đơn và mã người dùng 
            var order = await _appDbContext.PaymentOrders.FirstOrDefaultAsync(o => o.OrderCode == orderCode && o.UserId == userId, cancellationToken);

            return order;
        }

        /// <summary>
        /// Gets the payment order for the specified payment ID and user.
        /// </summary>
        /// <param name="paymentId">The payment identifier to match.</param>
        /// <param name="userId">The user identifier to match.</param>
        /// <param name="cancellationToken">A token used to cancel the operation.</param>
        /// <returns>The matching payment order, or <c>null</c> if no match is found.</returns>
        public async Task<PaymentOrder?> GetByIdAndUserIdAsync(Guid paymentId, Guid userId, CancellationToken cancellationToken = default)
        {
            var order = await _appDbContext.PaymentOrders.FirstOrDefaultAsync(o => o.Id == paymentId && o.UserId == userId, cancellationToken);

            return order;
        }

        /// <summary>
        /// Gets the payment order for the specified source order ID and user.
        /// </summary>
        /// <param name="orderId">The source order identifier to match.</param>
        /// <param name="userId">The user identifier to match.</param>
        /// <param name="cancellationToken">A token used to cancel the operation.</param>
        /// <returns>The matching payment order, or <c>null</c> if no match is found.</returns>
        public async Task<PaymentOrder?> GetByOrderIdAndUserIdAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default)
        {
            var order = await _appDbContext.PaymentOrders.FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId, cancellationToken);

            return order;
        }

        /// <summary>
        /// Gets the payment order with the specified order code.
        /// </summary>
        /// <param name="orderCode">The order code to match.</param>
        /// <param name="cancellationToken">A token used to cancel the operation.</param>
        /// <returns>The first payment order with the specified order code, or <c>null</c> if no match is found.</returns>
        public async Task<PaymentOrder?> GetByOrderCodeAsync(long orderCode, CancellationToken cancellationToken = default)
        {
            //Tìm đơn thanh toán theo mã đơn 
            var order = await _appDbContext.PaymentOrders.FirstOrDefaultAsync(o => o.OrderCode == orderCode, cancellationToken);

            return order;
        }
    }
}
