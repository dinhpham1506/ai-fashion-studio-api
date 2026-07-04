namespace AiFashionStudio.Platform.Domain.Identity;

/// <summary>
/// Pure domain model for an authenticated principal. No external dependencies.
/// </summary>
public sealed record User
{
    public Guid Id { get; }

    public string Email { get; }

    public string PasswordHash { get; }

    private User(Guid id, string email, string passwordHash)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
    }

    /// <summary>
    /// Factory that creates a new user with a generated identity.
    /// </summary>
    public static User Create(string email, string passwordHash)
        => new(Guid.NewGuid(), email, passwordHash);
}
