using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Domain.Content.Entities;
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

        public UpsertAboutUsSectionCommandHandler(IAboutUsContentRepository aboutUsContentRepository)
        {
            _aboutUsContentRepository = aboutUsContentRepository;
        }

        public async Task<UpsertAboutUsResponse> Handle(UpsertAboutUsSectionCommand command, CancellationToken cancellationToken)
        {
            // SectionKey chuẩn hóa về UPPERCASE để "introduction" và "INTRODUCTION" là cùng 1 section
            var sectionKey = command.SectionKey.Trim().ToUpperInvariant();
            var status = Enum.Parse<AboutUsStatus>(command.Status, ignoreCase: true);

            var section = await _aboutUsContentRepository.GetBySectionKeyAsync(sectionKey, cancellationToken);

            if (section is null)
            {
                section = AboutUsContent.Create(sectionKey, command.Title, command.Content, command.ImageUrl, status, command.UpdatedBy);
                await _aboutUsContentRepository.AddAsync(section, cancellationToken);
            }
            else
            {
                section.UpdateContent(command.Title, command.Content, command.ImageUrl, status, command.UpdatedBy);
                await _aboutUsContentRepository.SaveChangesAsync(cancellationToken);
            }

            return new UpsertAboutUsResponse(section.SectionKey, section.Status.ToString().ToUpperInvariant());
        }
    }
}
