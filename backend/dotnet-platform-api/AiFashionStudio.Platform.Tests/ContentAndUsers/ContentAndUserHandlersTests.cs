using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Application.Contents.Commands.UpsertAboutUsSection;
using AiFashionStudio.Platform.Application.Contents.Queries.GetPublishedAboutUs;
using AiFashionStudio.Platform.Application.Users.Commands.UpdateMyProfile;
using AiFashionStudio.Platform.Domain.Content.Entities;
using AiFashionStudio.Platform.Domain.Content.Enums;
using AiFashionStudio.Platform.Domain.Identity.Entities;
using AiFashionStudio.Platform.Domain.Identity.Enums;
using AiFashionStudio.Platform.Tests.Common;
using Xunit;

namespace AiFashionStudio.Platform.Tests.ContentAndUsers;

public class ContentAndUserHandlersTests
{
    [Fact]
    public async Task UpsertAboutUs_Should_Create_Uppercase_Section_Key()
    {
        var repository = new FakeAboutUsContentRepository();
        var handler = new UpsertAboutUsSectionCommandHandler(repository);

        var response = await handler.Handle(
            new UpsertAboutUsSectionCommand("intro", "Intro", "Content", null, "PUBLISHED", Guid.NewGuid()),
            CancellationToken.None);

        var section = Assert.Single(repository.Items);
        Assert.Equal("INTRO", section.SectionKey);
        Assert.Equal("PUBLISHED", response.Status);
    }

    [Fact]
    public async Task UpsertAboutUs_Should_Update_Existing_Section()
    {
        var section = AboutUsContent.Create("MISSION", "Old", "Old content", null, AboutUsStatus.Draft, Guid.NewGuid());
        var repository = new FakeAboutUsContentRepository(section);
        var handler = new UpsertAboutUsSectionCommandHandler(repository);

        var response = await handler.Handle(
            new UpsertAboutUsSectionCommand("mission", "New", "New content", "image.png", "PUBLISHED", Guid.NewGuid()),
            CancellationToken.None);

        Assert.Single(repository.Items);
        Assert.Equal("MISSION", response.SectionKey);
        Assert.Equal("New", section.Title);
        Assert.Equal("image.png", section.ImageUrl);
        Assert.Equal(1, repository.SaveChangesCount);
    }

    [Fact]
    public async Task GetPublishedAboutUs_Should_Return_Only_Repository_Published_Sections()
    {
        var repository = new FakeAboutUsContentRepository(
            AboutUsContent.Create("INTRO", "Intro", "Content", null, AboutUsStatus.Published, Guid.NewGuid()));
        var handler = new GetPublishedAboutUsQueryHandler(repository);

        var response = await handler.Handle(new GetPublishedAboutUsQuery(), CancellationToken.None);

        var section = Assert.Single(response);
        Assert.Equal("INTRO", section.SectionKey);
        Assert.Equal("Intro", section.Title);
    }

    [Fact]
    public async Task UpdateMyProfile_Should_Trim_Name_And_Blank_Phone()
    {
        var role = Role.Create(RoleName.Customer, "Customer");
        var user = User.Register("profile@example.com", "hash", "Old Name", "123");
        user.AssignRole(role);
        foreach (var userRole in user.UserRoles)
        {
            TestReflection.SetPrivateProperty(userRole, nameof(UserRole.Role), role);
        }

        var repository = new FakeUserRepository(user);
        var handler = new UpdateMyProfileCommandHandler(repository);

        var response = await handler.Handle(
            new UpdateMyProfileCommand(user.Id, "  New Name  ", "   "),
            CancellationToken.None);

        Assert.Equal("New Name", response.FullName);
        Assert.Null(response.Phone);
        Assert.Equal(1, repository.SaveChangesCount);
    }

    private sealed class FakeAboutUsContentRepository : InMemoryRepository<AboutUsContent>, IAboutUsContentRepository
    {
        public FakeAboutUsContentRepository(params AboutUsContent[] sections)
        {
            Store.AddRange(sections);
        }

        public Task<List<AboutUsContent>> GetPublishedAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Store.Where(section => section.Status == AboutUsStatus.Published).ToList());

        public Task<AboutUsContent?> GetBySectionKeyAsync(string sectionKey, CancellationToken cancellationToken = default)
            => Task.FromResult(Store.FirstOrDefault(section => section.SectionKey == sectionKey));

        public async Task<AboutUsContent> UpsertSectionAsync(
            string sectionKey,
            string title,
            string content,
            string? imageUrl,
            AboutUsStatus status,
            Guid updatedBy,
            CancellationToken cancellationToken = default)
        {
            var section = Store.FirstOrDefault(item => item.SectionKey == sectionKey);
            if (section is null)
            {
                section = AboutUsContent.Create(sectionKey, title, content, imageUrl, status, updatedBy);
                await AddAsync(section, cancellationToken);
                return section;
            }

            section.UpdateContent(title, content, imageUrl, status, updatedBy);
            await SaveChangesAsync(cancellationToken);
            return section;
        }
    }

    private sealed class FakeUserRepository : InMemoryRepository<User>, IUserRepository
    {
        public FakeUserRepository(params User[] users)
        {
            Store.AddRange(users);
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            => Task.FromResult(Store.FirstOrDefault(user => user.Email == email));

        public Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Store.FirstOrDefault(user => user.Id == id));

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
            => Task.FromResult(Store.Any(user => user.Email == email));
    }
}
