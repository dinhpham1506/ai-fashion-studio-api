using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;
using System;

namespace AiFashionStudio.Platform.Application.Contents.Commands.UpsertAboutUsSection
{
    /// <summary>
    /// Tạo mới hoặc cập nhật một section About Us — Status nhận "DRAFT" hoặc "PUBLISHED"
    /// </summary>
    public record UpsertAboutUsSectionCommand(
        string SectionKey, string Title, string Content,
        string? ImageUrl, string Status, Guid UpdatedBy) : IRequest<UpsertAboutUsResponse>;
}
