using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;
using System.Collections.Generic;

namespace AiFashionStudio.Platform.Application.Contents.Queries.GetPublishedAboutUs
{
    public record GetPublishedAboutUsQuery() : IRequest<IReadOnlyCollection<AboutUsSectionResponse>>;
}
