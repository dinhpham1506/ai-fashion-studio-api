using AiFashionStudio.Platform.Domain.Common;
using AiFashionStudio.Platform.Domain.Identity.Enums;

namespace AiFashionStudio.Platform.Domain.Identity.Entities;

/// <summary>
///  dữ liệu người dùng, bao gồm email, mật khẩu, tên đầy đủ, số điện thoại, trạng thái và danh sách các vai trò
/// </summary>
public class User : UpdatableEntity
{
    // danh sách các vai trò của người dùng
    private readonly List<UserRole> _userRoles = [];

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string FullName { get; private set; } = string.Empty;

    public string? Phone { get; private set; }

    // ảnh đại diện của người dùng
    public string? AvatarUrl { get; private set; }

    // trạng thái của người dùng, mặc định là Active
    public UserStatus Status { get; private set; } = UserStatus.Active;

    // danh sách các vai trò của người dùng
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    
    private User()
    {
    }

    
    private User(string email, string passwordHash, string fullName, string? phone)
    {
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        Phone = phone;
        Status = UserStatus.Active;
    }

    // Tạo đăng ký người dùng mới
    public static User Register(string email, string passwordHash, string fullName, string? phone = null)
        => new(email, passwordHash, fullName, phone);

    // Gán vai trò cho người dùng, nếu người dùng đã có vai trò đó thì không làm gì cả
    public void AssignRole(Role role)
    {
        if (_userRoles.Any(userRole => userRole.RoleId == role.Id))
        {
            return;
        }

        _userRoles.Add(UserRole.Create(Id, role.Id));
    }

    // Kiểm tra xem người dùng có bị ban hay không
    public bool IsActive() => Status == UserStatus.Active;

    // Đổi mật khẩu của người dùng, cập nhật PasswordHash và UpdatedAt
    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        Update();
    }
}
