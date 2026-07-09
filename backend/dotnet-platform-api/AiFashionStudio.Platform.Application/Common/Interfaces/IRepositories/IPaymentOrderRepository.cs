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

        Task<PaymentOrder?> GetByOrderCodeAsync(long orderCode, CancellationToken cancellationToken = default);

        // Kèm check ownership — dùng cho GET/cancel của customer
        Task<PaymentOrder?> GetByOrderCodeAndUserIdAsync(long orderCode, Guid userId, CancellationToken cancellationToken = default);
    }
}
