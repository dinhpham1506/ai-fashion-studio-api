using AiFashionStudio.Platform.Domain.Common;
using AiFashionStudio.Platform.Domain.Identity.Enums;

namespace AiFashionStudio.Platform.Domain.Identity.Entities;

/// <summary>
/// dũ liệu role của user
/// </summary>
public class Role : BaseEntity
{
    public RoleName Code { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    private Role()
    {
    }

    private Role(RoleName code, string name, string? description)
    {
        Code = code;
        Name = name;
        Description = description;
    }

    public static Role Create(RoleName code, string name, string? description = null)
        => new(code, name, description);
}
