using System;
using System.Collections.Generic;

namespace AiFashionStudio.Platform.Application.Common.Dtos
{
    public record AboutUsSectionResponse(string SectionKey, string Title, string Content, string? ImageUrl);

    public record UpsertAboutUsResponse(string SectionKey, string Status);
}
