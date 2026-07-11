using AiFashionStudio.Platform.Api.Common;
using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Payments.Commands;
using AiFashionStudio.Platform.Application.Payments.Commands.CancelPayment;
using AiFashionStudio.Platform.Application.Payments.Commands.ProcessPaymentWebhook;
using AiFashionStudio.Platform.Application.Payments.Queries.GetPaymentStatus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiFashionStudio.Platform.Api.Controllers;

public record CreatePaymentRequest(int Amount, string Description);

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentsController"/> class.
    /// </summary>
    public PaymentsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Creates a payment link for the authenticated user.
    /// </summary>
    /// <param name="request">The payment creation details.</param>
    /// <returns>An unauthorized result when the token is invalid; otherwise, a created result containing the payment link.</returns>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse.Fail("Invalid token", Array.Empty<ApiError>()));
        }

        var result = await _sender.Send(
            new CreatePaymentCommand(userId, request.Amount, request.Description), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse.Ok(result, "Payment link created"));
    }

    /// <summary>
    /// Processes a payment webhook request.
    /// </summary>
    /// <returns>A success response indicating that the webhook was processed.</returns>
    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);

        try
        {
            await _sender.Send(new ProcessPaymentWebhookCommand(rawBody), cancellationToken);
            return Ok(ApiResponse.Ok("Webhook processed"));
        }
        catch (WebhookVerificationException exception)
        {
            var errors = exception.Errors
                .Select(error => new ApiError(error.Code, error.Message, error.Field))
                .ToArray();

            return BadRequest(ApiResponse.Fail(exception.Message, errors));
        }
    }

    /// <summary>
    /// Gets the payment status for an order.
    /// </summary>
    /// <param name="orderCode">The order code to look up.</param>
    /// <returns>An <see cref="IActionResult"/> containing the payment status, or an unauthorized response if the token is invalid.</returns>
    [Authorize]
    [HttpGet("{orderCode:long}")]
    public async Task<IActionResult> GetStatus(long orderCode, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse.Fail("Invalid token", Array.Empty<ApiError>()));
        }

        var result = await _sender.Send(new GetPaymentStatusQuery(userId, orderCode), cancellationToken);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// Cancels a payment for the authenticated user.
    /// </summary>
    /// <param name="orderCode">The payment order code.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>An action result containing a success message, or an unauthorized result when the token is invalid.</returns>
    [Authorize]
    [HttpPost("{orderCode:long}/cancel")]
    public async Task<IActionResult> Cancel(long orderCode, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse.Fail("Invalid token", Array.Empty<ApiError>()));
        }

        await _sender.Send(new CancelPaymentCommand(userId, orderCode), cancellationToken);
        return Ok(ApiResponse.Ok("Payment cancelled"));
    }

    /// <summary>
    /// Reads the user identifier from the <c>sub</c> claim.
    /// </summary>
    /// <param name="userId">When this method returns, contains the parsed user identifier or <see cref="Guid.Empty"/>.</param>
    /// <returns><c>true</c> if the <c>sub</c> claim contains a valid identifier, <c>false</c> otherwise.</returns>
    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var subject = User.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value;
        return subject is not null && Guid.TryParse(subject, out userId);
    }
}
