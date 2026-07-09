using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Domain.Content.Entities;
using AiFashionStudio.Platform.Domain.Content.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Repositories
{
    public class AboutUsContentRepository : BaseRepository<AboutUsContent>, IAboutUsContentRepository
    {
        private readonly AppDbContext _appDbContext;

        public AboutUsContentRepository(AppDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public Task<List<AboutUsContent>> GetPublishedAsync(CancellationToken cancellationToken = default)
            => _appDbContext.AboutUsContents
                .Where(section => section.Status == AboutUsStatus.Published)
                .OrderBy(section => section.SectionKey)
                .ToListAsync(cancellationToken);

        public Task<AboutUsContent?> GetBySectionKeyAsync(string sectionKey, CancellationToken cancellationToken = default)
            => _appDbContext.AboutUsContents
                .FirstOrDefaultAsync(section => section.SectionKey == sectionKey, cancellationToken);
    }
}
