using AiFashionStudio.Platform.Domain.Content.Entities;
using AiFashionStudio.Platform.Domain.Content.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories
{
    public interface IAboutUsContentRepository : IBaseRepository<AboutUsContent>
    {
        /// <summary>
/// Gets the published About Us sections.
/// </summary>
/// <returns>The published About Us content entries.</returns>
        Task<List<AboutUsContent>> GetPublishedAsync(CancellationToken cancellationToken = default);

        /// <summary>
/// Gets an about-us content entry by section key.
/// </summary>
/// <param name="sectionKey">The section key to match.</param>
/// <param name="cancellationToken">A token that supports cooperative cancellation.</param>
/// <returns>The matching about-us content entry, or null if no match is found.</returns>
        Task<AboutUsContent?> GetBySectionKeyAsync(string sectionKey, CancellationToken cancellationToken = default);

        Task<AboutUsContent> UpsertSectionAsync(
            string sectionKey,
            string title,
            string content,
            string? imageUrl,
            AboutUsStatus status,
            Guid updatedBy,
            CancellationToken cancellationToken = default);
    }
}
