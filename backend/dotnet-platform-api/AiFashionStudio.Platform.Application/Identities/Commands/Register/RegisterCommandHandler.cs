using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Domain.Identity.Entities;
using AiFashionStudio.Platform.Domain.Identity.Enums;
using MediatR;

namespace AiFashionStudio.Platform.Application.Identities.Commands.Register;
/// <summary>
/// Xử lý logic đăng ký người dùng mới, bao gồm kiểm tra email tồn tại, tạo người dùng mới, gán vai trò và lưu vào cơ sở dữ liệu.
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        if (await _userRepository.ExistsByEmailAsync(command.Email, cancellationToken))
        {
            throw new ConflictException("EMAIL_ALREADY_EXISTS", "Email already exists");
        }
        // Lấy vai trò mặc định cho người dùng mới
        var role = await _roleRepository.GetByCodeAsync(RoleName.Customer, cancellationToken)
            ?? throw new InvalidOperationException($"Role '{RoleName.Customer}' is not seeded.");

        var passwordHash = _passwordHasher.Hash(command.Password);
        // Tạo người dùng mới với thông tin từ lệnh đăng ký
        var user = User.Register(command.Email, passwordHash, command.FullName, command.Phone);
        // Gán vai trò mặc định cho người dùng mới
        user.AssignRole(role);

        await _userRepository.AddAsync(user, cancellationToken);
    }
}
