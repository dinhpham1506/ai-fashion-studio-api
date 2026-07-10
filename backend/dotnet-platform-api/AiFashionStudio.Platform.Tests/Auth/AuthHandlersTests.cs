using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using AiFashionStudio.Platform.Application.Identities.Commands.Login;
using AiFashionStudio.Platform.Application.Identities.Commands.Logout;
using AiFashionStudio.Platform.Application.Identities.Commands.RefreshAccessToken;
using AiFashionStudio.Platform.Application.Identities.Commands.Register;
using AiFashionStudio.Platform.Domain.Identity.Entities;
using AiFashionStudio.Platform.Domain.Identity.Enums;
using AiFashionStudio.Platform.Tests.Common;
using Xunit;

namespace AiFashionStudio.Platform.Tests.Auth;

public class AuthHandlersTests
{
    [Fact]
    public async Task Register_Should_Create_User_With_Customer_Role()
    {
        var users = new FakeUserRepository();
        var roles = new FakeRoleRepository(Role.Create(RoleName.Customer, "Customer"));
        var handler = new RegisterCommandHandler(users, roles, new FakePasswordHasher());

        await handler.Handle(new RegisterCommand("new@example.com", "password123", "New User", "0909"), CancellationToken.None);

        var user = Assert.Single(users.Items);
        Assert.Equal("new@example.com", user.Email);
        Assert.Equal("hashed:password123", user.PasswordHash);
        Assert.Contains(user.UserRoles, role => role.RoleId == roles.CustomerRole.Id);
    }

    [Fact]
    public async Task Register_Should_Reject_Duplicate_Email()
    {
        var existing = User.Register("used@example.com", "hash", "Used User");
        var users = new FakeUserRepository(existing);
        var roles = new FakeRoleRepository(Role.Create(RoleName.Customer, "Customer"));
        var handler = new RegisterCommandHandler(users, roles, new FakePasswordHasher());

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(new RegisterCommand("used@example.com", "password123", "Other", null), CancellationToken.None));

        Assert.Equal("EMAIL_ALREADY_EXISTS", exception.Errors.Single().Code);
        Assert.Single(users.Items);
    }

    [Fact]
    public async Task Login_Should_Reject_Invalid_Credentials()
    {
        var user = User.Register("login@example.com", "hashed:correct", "Login User");
        var handler = new LoginCommandHandler(
            new FakeUserRepository(user),
            new FakeRefreshTokenRepository(),
            new FakePasswordHasher(),
            new FakeTokenService());

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.Handle(new LoginCommand("login@example.com", "wrong"), CancellationToken.None));

        Assert.Equal("INVALID_CREDENTIALS", exception.Errors.Single().Code);
    }

    [Fact]
    public async Task Login_Should_Return_Tokens_And_Save_Refresh_Token()
    {
        var customerRole = Role.Create(RoleName.Customer, "Customer");
        var user = User.Register("login@example.com", "hashed:password123", "Login User");
        user.AssignRole(customerRole);
        AttachRoleNavigation(user, customerRole);

        var refreshTokens = new FakeRefreshTokenRepository();
        var handler = new LoginCommandHandler(
            new FakeUserRepository(user),
            refreshTokens,
            new FakePasswordHasher(),
            new FakeTokenService());

        var result = await handler.Handle(new LoginCommand("login@example.com", "password123"), CancellationToken.None);

        Assert.Equal("access-token", result.Response.AccessToken);
        Assert.Equal("refresh-token-1", result.RefreshToken);
        Assert.Contains("CUSTOMER", result.Response.User.Roles);
        Assert.Single(refreshTokens.Items);
        Assert.Equal("refresh-hash:refresh-token-1", refreshTokens.Items.Single().TokenHash);
    }

    [Fact]
    public async Task RefreshAccessToken_Should_Revoke_Old_Token_And_Create_New_Token()
    {
        var customerRole = Role.Create(RoleName.Customer, "Customer");
        var user = User.Register("refresh@example.com", "hashed:password123", "Refresh User");
        user.AssignRole(customerRole);
        AttachRoleNavigation(user, customerRole);

        var existingToken = RefreshToken.Create(user.Id, "refresh-hash:old-token", DateTime.UtcNow.AddHours(1));
        var refreshTokens = new FakeRefreshTokenRepository(existingToken);
        var handler = new RefreshAccessTokenCommandHandler(
            new FakeUserRepository(user),
            refreshTokens,
            new FakeTokenService());

        var result = await handler.Handle(new RefreshAccessTokenCommand("old-token"), CancellationToken.None);

        Assert.NotNull(existingToken.RevokedAt);
        Assert.Equal("refresh-token-1", result.RefreshToken);
        Assert.Equal(2, refreshTokens.Items.Count);
    }

    [Fact]
    public async Task Logout_Should_Revoke_Existing_Refresh_Token()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "refresh-hash:logout-token", DateTime.UtcNow.AddHours(1));
        var refreshTokens = new FakeRefreshTokenRepository(token);
        var handler = new LogoutCommandHandler(refreshTokens, new FakeTokenService());

        await handler.Handle(new LogoutCommand("logout-token"), CancellationToken.None);

        Assert.NotNull(token.RevokedAt);
        Assert.Equal(1, refreshTokens.UpdateCount);
    }

    private static void AttachRoleNavigation(User user, Role role)
    {
        foreach (var userRole in user.UserRoles)
        {
            TestReflection.SetPrivateProperty(userRole, nameof(UserRole.Role), role);
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"hashed:{password}";

        public bool Verify(string password, string passwordHash) => passwordHash == Hash(password);
    }

    private sealed class FakeTokenService : ITokenService
    {
        private int _refreshCounter;

        public TimeSpan RefreshTokenLifetime => TimeSpan.FromDays(7);

        public AccessTokenResult GenerateAccessToken(User user, IReadOnlyCollection<string> roleCodes)
            => new("access-token", 3600);

        public string GenerateRefreshToken()
        {
            _refreshCounter++;
            return $"refresh-token-{_refreshCounter}";
        }

        public string HashRefreshToken(string refreshToken) => $"refresh-hash:{refreshToken}";
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

    private sealed class FakeRoleRepository : InMemoryRepository<Role>, IRoleRepository
    {
        public FakeRoleRepository(Role customerRole)
        {
            CustomerRole = customerRole;
            Store.Add(customerRole);
        }

        public Role CustomerRole { get; }

        public Task<Role?> GetByCodeAsync(RoleName code, CancellationToken cancellationToken = default)
            => Task.FromResult(Store.FirstOrDefault(role => role.Code == code));
    }

    private sealed class FakeRefreshTokenRepository : InMemoryRepository<RefreshToken>, IRefreshTokenRepository
    {
        public FakeRefreshTokenRepository(params RefreshToken[] tokens)
        {
            Store.AddRange(tokens);
        }

        public int UpdateCount { get; private set; }

        public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
            => Task.FromResult(Store.FirstOrDefault(token => token.TokenHash == tokenHash));

        public Task RevokeAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            foreach (var token in Store.Where(token => token.UserId == userId && token.RevokedAt is null))
            {
                token.Revoke();
            }

            return Task.CompletedTask;
        }

        public new Task UpdateAsync(RefreshToken entity, CancellationToken cancellationToken = default)
        {
            UpdateCount++;
            return base.UpdateAsync(entity, cancellationToken);
        }
    }
}
