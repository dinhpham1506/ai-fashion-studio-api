using AiFashionStudio.Platform.Domain.Common;
using AiFashionStudio.Platform.Domain.Content.Enums;

namespace AiFashionStudio.Platform.Domain.Content.Entities
{
    /// <summary>
    /// Nội dung một section trong trang About Us (INTRODUCTION, MISSION, HOW_IT_WORKS...)
    /// </summary>
    public class AboutUsContent : UpdatableEntity
    {
        public string SectionKey { get; private set; } = default!;
        public string Title { get; private set; } = default!;
        public string Content { get; private set; } = default!;
        public string? ImageUrl { get; private set; }
        public AboutUsStatus Status { get; private set; } = AboutUsStatus.Draft;
        public Guid? UpdatedBy { get; private set; }

        private AboutUsContent()
        {
        }

        /// <summary>
        /// Creates a new About Us content section.
        /// </summary>
        /// <param name="sectionKey">The section identifier.</param>
        /// <param name="title">The section title.</param>
        /// <param name="content">The section body content.</param>
        /// <param name="imageUrl">The image URL for the section, if any.</param>
        /// <param name="status">The content status.</param>
        /// <param name="updatedBy">The identifier of the user creating the content.</param>
        /// <returns>The created About Us content section.</returns>
        public static AboutUsContent Create(string sectionKey, string title, string content, string? imageUrl, AboutUsStatus status, Guid updatedBy)
        {
            return new AboutUsContent
            {
                SectionKey = sectionKey,
                Title = title,
                Content = content,
                ImageUrl = imageUrl,
                Status = status,
                UpdatedBy = updatedBy
            };
        }

        /// <summary>
        /// Updates the section content and audit fields.
        /// </summary>
        /// <param name="title">The updated section title.</param>
        /// <param name="content">The updated section body content.</param>
        /// <param name="imageUrl">The updated image URL for the section.</param>
        /// <param name="status">The updated content status.</param>
        /// <param name="updatedBy">The identifier of the user who made the update.</param>
        public void UpdateContent(string title, string content, string? imageUrl, AboutUsStatus status, Guid updatedBy)
        {
            Title = title;
            Content = content;
            ImageUrl = imageUrl;
            Status = status;
            UpdatedBy = updatedBy;
            Update();
        }
    }
}
