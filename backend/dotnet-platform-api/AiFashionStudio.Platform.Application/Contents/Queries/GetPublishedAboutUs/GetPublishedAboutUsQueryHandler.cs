using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Contents.Queries.GetPublishedAboutUs
{
    public class GetPublishedAboutUsQueryHandler : IRequestHandler<GetPublishedAboutUsQuery, IReadOnlyCollection<AboutUsSectionResponse>>
    {
        private readonly IAboutUsContentRepository _aboutUsContentRepository;

        public GetPublishedAboutUsQueryHandler(IAboutUsContentRepository aboutUsContentRepository)
        {
            _aboutUsContentRepository = aboutUsContentRepository;
        }

        public async Task<IReadOnlyCollection<AboutUsSectionResponse>> Handle(GetPublishedAboutUsQuery request, CancellationToken cancellationToken)
        {
            var sections = await _aboutUsContentRepository.GetPublishedAsync(cancellationToken);

            return sections
                .Select(section => new AboutUsSectionResponse(
                    section.SectionKey, section.Title, section.Content, section.ImageUrl))
                .ToList();
        }
    }
}
