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
        // Tìm invoice theo order — mỗi order chỉ có 1 invoice
        Task<Invoice?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

        // Idempotency: ngăn tạo 2 invoice cho cùng 1 order
        Task<bool> ExistsForOrderAsync(Guid orderId, CancellationToken cancellationToken = default);

        // Đếm số invoice đã phát hành trong ngày, dùng để sinh số thứ tự cho invoiceNumber
        Task<int> CountIssuedTodayAsync(DateOnly issuedDate, CancellationToken cancellationToken = default);
    }
}
