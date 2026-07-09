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
