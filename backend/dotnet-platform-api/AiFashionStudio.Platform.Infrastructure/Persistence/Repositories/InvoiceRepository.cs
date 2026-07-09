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

        public InvoiceRepository(AppDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        // Override để load kèm Items — GetByIdAsync mặc định của base không Include
        public override Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _appDbContext.Invoices.Include(invoice => invoice.Items)
                .FirstOrDefaultAsync(invoice => invoice.Id == id, cancellationToken);

        public Task<Invoice?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
            => _appDbContext.Invoices.Include(invoice => invoice.Items)
                .FirstOrDefaultAsync(invoice => invoice.OrderId == orderId, cancellationToken);

        public Task<bool> ExistsForOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
            => _appDbContext.Invoices.AnyAsync(invoice => invoice.OrderId == orderId, cancellationToken);

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
