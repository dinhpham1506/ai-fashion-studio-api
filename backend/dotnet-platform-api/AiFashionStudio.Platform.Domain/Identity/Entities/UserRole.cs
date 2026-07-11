using AiFashionStudio.Platform.Domain.Common;

namespace AiFashionStudio.Platform.Domain.Identity.Entities;

public class UserRole : BaseEntity
{
    public Guid UserId { get; private set; }

    public Guid RoleId { get; private set; }

    public Role Role { get; private set; } = null!;

    private UserRole()
    {
    }

    private UserRole(Guid userId, Guid roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }

    public static UserRole Create(Guid userId, Guid roleId) => new(userId, roleId);
}
