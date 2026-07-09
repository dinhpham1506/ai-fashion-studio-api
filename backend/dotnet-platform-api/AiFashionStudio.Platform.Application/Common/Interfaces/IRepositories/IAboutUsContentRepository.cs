using AiFashionStudio.Platform.Domain.Content.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories
{
    public interface IAboutUsContentRepository : IBaseRepository<AboutUsContent>
    {
        // Lấy các section PUBLISHED cho trang public
        Task<List<AboutUsContent>> GetPublishedAsync(CancellationToken cancellationToken = default);

        // Tìm section theo key — dùng cho upsert
        Task<AboutUsContent?> GetBySectionKeyAsync(string sectionKey, CancellationToken cancellationToken = default);
    }
}
