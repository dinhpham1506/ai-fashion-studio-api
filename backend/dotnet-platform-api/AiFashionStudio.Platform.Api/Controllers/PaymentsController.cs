using AiFashionStudio.Platform.Api.Common;
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

    public PaymentsController(ISender sender)
    {
        _sender = sender;
    }

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

    // PayOS gọi endpoint này — phải đọc RAW body để verify chữ ký
    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);

        await _sender.Send(new ProcessPaymentWebhookCommand(rawBody), cancellationToken);
        return Ok(ApiResponse.Ok("Webhook processed"));
    }

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

    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var subject = User.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value;
        return subject is not null && Guid.TryParse(subject, out userId);
    }
}