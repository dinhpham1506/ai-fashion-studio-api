using AiFashionStudio.Platform.Application.Common.Exceptions;
using FluentValidation;
using MediatR;

namespace AiFashionStudio.Platform.Application.Common.Behaviors;

/// <summary>
/// chạy trước khi xử lý request, kiểm tra xem request có hợp lệ hay không
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count > 0)
        {
            var errors = failures
                .Select(failure => new AppError(failure.ErrorCode, failure.ErrorMessage, failure.PropertyName))
                .ToList();

            throw new AppValidationException(errors);
        }

        return await next();
    }
}
