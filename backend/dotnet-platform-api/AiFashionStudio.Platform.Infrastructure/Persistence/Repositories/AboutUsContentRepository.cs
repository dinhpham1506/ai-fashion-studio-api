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

        /// <summary>
        /// Initializes a new instance of the <see cref="AboutUsContentRepository"/> class.
        /// </summary>
        /// <param name="appDbContext">The database context used for data access.</param>
        public AboutUsContentRepository(AppDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        /// <summary>
                /// Gets all published about-us content entries ordered by section key.
                /// </summary>
                /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
                /// <returns>The published about-us content entries ordered by section key.</returns>
                public Task<List<AboutUsContent>> GetPublishedAsync(CancellationToken cancellationToken = default)
            => _appDbContext.AboutUsContents
                .Where(section => section.Status == AboutUsStatus.Published)
                .OrderBy(section => section.SectionKey)
                .ToListAsync(cancellationToken);

        /// <summary>
                /// Gets the about-us content for a section key.
                /// </summary>
                /// <param name="sectionKey">The section key to match.</param>
                /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
                /// <returns>The matching <see cref="AboutUsContent"/> if found; otherwise, <c>null</c>.</returns>
                public Task<AboutUsContent?> GetBySectionKeyAsync(string sectionKey, CancellationToken cancellationToken = default)
            => _appDbContext.AboutUsContents
                .FirstOrDefaultAsync(section => section.SectionKey == sectionKey, cancellationToken);
    }
}
