using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Domain.Content.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Contents.Commands.UpsertAboutUsSection
{
    public class UpsertAboutUsSectionCommandHandler : IRequestHandler<UpsertAboutUsSectionCommand, UpsertAboutUsResponse>
    {
        private readonly IAboutUsContentRepository _aboutUsContentRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpsertAboutUsSectionCommandHandler"/> class.
        /// </summary>
        /// <param name="aboutUsContentRepository">The repository used to access About Us content.</param>
        public UpsertAboutUsSectionCommandHandler(IAboutUsContentRepository aboutUsContentRepository)
        {
            _aboutUsContentRepository = aboutUsContentRepository;
        }

        /// <summary>
        /// Upserts an About Us section.
        /// </summary>
        /// <param name="command">The section data to create or update.</param>
        /// <param name="cancellationToken">A token that can cancel the operation.</param>
        /// <returns>The section key and status after the upsert.</returns>
        public async Task<UpsertAboutUsResponse> Handle(UpsertAboutUsSectionCommand command, CancellationToken cancellationToken)
        {
            // SectionKey chuẩn hóa về UPPERCASE để "introduction" và "INTRODUCTION" là cùng 1 section
            var sectionKey = command.SectionKey.Trim().ToUpperInvariant();
            var status = Enum.Parse<AboutUsStatus>(command.Status, ignoreCase: true);

            var section = await _aboutUsContentRepository.UpsertSectionAsync(
                sectionKey,
                command.Title,
                command.Content,
                command.ImageUrl,
                status,
                command.UpdatedBy,
                cancellationToken);

            return new UpsertAboutUsResponse(section.SectionKey, section.Status.ToString().ToUpperInvariant());
        }
    }
}
