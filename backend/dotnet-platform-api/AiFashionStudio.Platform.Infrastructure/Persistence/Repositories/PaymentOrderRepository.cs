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

        public PaymentOrderRepository(AppDbContext appDbContext) : base(appDbContext)
        {
            
                _appDbContext = appDbContext;
            
        }

        public async Task<PaymentOrder?> GetByOrderCodeAndUserIdAsync(long orderCode, Guid userId, CancellationToken cancellationToken = default)
        {
            //Tìm đơn thanh toán theo mã đơn và mã người dùng 
            var order = await _appDbContext.PaymentOrders.FirstOrDefaultAsync(o => o.OrderCode == orderCode && o.UserId == userId, cancellationToken);

            return order;
        }

        public async Task<PaymentOrder?> GetByOrderCodeAsync(long orderCode, CancellationToken cancellationToken = default)
        {
            //Tìm đơn thanh toán theo mã đơn 
            var order = await _appDbContext.PaymentOrders.FirstOrDefaultAsync(o => o.OrderCode == orderCode, cancellationToken);

            return order;
        }
    }
}
