using System.Runtime.CompilerServices;

namespace AiFashionStudio.Platform.Application.Common.Exceptions;

public record AppError(string Code, string Message, string? Field = null);

/// <summary>
/// bắt buộc tất cả các exception trong ứng dụng phải kế thừa từ AppException để đảm bảo rằng tất cả các exception đều có thể được xử lý một cách nhất quán và có thể trả về thông tin lỗi cho client.
/// </summary>
public abstract class AppException : Exception
{
    protected AppException(string message, IReadOnlyCollection<AppError> errors)
        : base(message)
    {
        Errors = errors;
    }

    public IReadOnlyCollection<AppError> Errors { get; }
}

public class AppValidationException : AppException
{
    public AppValidationException(string field, string code, string message)
        : base(message, new[] { new AppError(code, message, field) })
    {
    }

    public AppValidationException(IReadOnlyCollection<AppError> errors)
        : base("Validation failed", errors)
    {
    }
}
// lỗi xảy ra xung đột khi người dùng cố gắng thực hiện một hành động mà không hợp lệ, ví dụ như tạo một tài nguyên đã tồn tại.
public class ConflictException : AppException
{
    public ConflictException(string code, string message)
        : base(message, new[] { new AppError(code, message) })
    {
    }
}

// lỗi xảy ra khi người dùng không có quyền truy cập vào tài nguyên
public class UnauthorizedException : AppException
{
    public UnauthorizedException(string code, string message)
        : base(message, new[] { new AppError(code, message) })
    {
    }
}
// lỗi xảy ra khi người dùng không có quyền truy cập vào tài nguyên
public class ForbiddenException : AppException
{
    /// <summary>
    /// Creates an exception for a forbidden access error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    public ForbiddenException(string code, string message)
        : base(message, new[] { new AppError(code, message) })
    {
    }

}
public class NotFoundException : AppException
{
    /// <summary>
    /// Creates an exception for a missing resource.
    /// </summary>
    /// <param name="code">The application error code.</param>
    /// <param name="message">The error message.</param>
    public NotFoundException(string code, string message) : base(message, new[] { new AppError(code, message) })
    {

    }
}