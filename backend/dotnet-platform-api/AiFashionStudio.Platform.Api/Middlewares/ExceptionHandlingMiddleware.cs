using System.Net;
using AiFashionStudio.Platform.Api.Common;
using AiFashionStudio.Platform.Application.Common.Exceptions;

namespace AiFashionStudio.Platform.Api.Middlewares;

/// <summary>
/// bắt lỗi toàn cục cho ứng dụng, xử lý các ngoại lệ và trả về phản hồi lỗi phù hợp.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    // lỗi được xử lý theo thứ tự ưu tiên: AppException -> Exception 
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException exception)
        {
            await WriteErrorAsync(context, MapStatusCode(exception), exception.Message, exception.Errors);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception");

            var errors = _environment.IsDevelopment()
                ? CollectErrors(exception)
                : new[] { new AppError("INTERNAL_SERVER_ERROR", "Something went wrong") };

            await WriteErrorAsync(
                context,
                HttpStatusCode.InternalServerError,
                _environment.IsDevelopment() ? exception.Message : "Internal server error",
                errors);
        }
    }

    private static IReadOnlyCollection<AppError> CollectErrors(Exception exception)
    {
        var errors = new List<AppError>();

        for (var current = exception; current is not null; current = current.InnerException)
        {
            errors.Add(new AppError(current.GetType().Name, current.Message));
        }

        return errors;
    }

    private static HttpStatusCode MapStatusCode(AppException exception) => exception switch
    {
        AppValidationException => HttpStatusCode.BadRequest,
        UnauthorizedException => HttpStatusCode.Unauthorized,
        ForbiddenException => HttpStatusCode.Forbidden,
        ConflictException => HttpStatusCode.Conflict,
        _ => HttpStatusCode.InternalServerError
    };

    private static async Task WriteErrorAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string message,
        IReadOnlyCollection<AppError> errors)
    {
        var apiErrors = errors
            .Select(error => new ApiError(error.Code, error.Message, error.Field))
            .ToList();

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsJsonAsync(ApiResponse.Fail(message, apiErrors));
    }
}
