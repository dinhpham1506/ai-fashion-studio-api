using AiFashionStudio.Platform.Api.Common;
using AiFashionStudio.Platform.Application.AiChat.Commands.ResolveAiChatConversation;
using AiFashionStudio.Platform.Application.AiChat.Commands.SendAiChatMessage;
using AiFashionStudio.Platform.Application.AiChat.Commands.StartAiChatConversation;
using AiFashionStudio.Platform.Application.AiChat.Queries.GetAiChatConversation;
using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiFashionStudio.Platform.Api.Controllers;

public record StartAiChatConversationRequest(
    string? Channel,
    AiChatPageContext? Page);

public record SendAiChatMessageRequest(
    string Message,
    AiChatPageContext? Page,
    AiChatClientContext? ClientContext);

[ApiController]
[Route("api/ai-chat")]
public class AiChatController : ControllerBase
{
    private readonly ISender _sender;

    public AiChatController(ISender sender)
    {
        _sender = sender;
    }

    [AllowAnonymous]
    [HttpPost("conversations")]
    public async Task<IActionResult> Start(
        [FromBody] StartAiChatConversationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new StartAiChatConversationCommand(
                TryGetUserId(out var userId) ? userId : null,
                TryGetUserRole(),
                request.Channel,
                request.Page),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ApiResponse.Ok(result, "AI chat conversation created"));
    }

    [AllowAnonymous]
    [HttpPost("conversations/{conversationId:guid}/messages")]
    public async Task<IActionResult> SendMessage(
        Guid conversationId,
        [FromBody] SendAiChatMessageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new SendAiChatMessageCommand(
                conversationId,
                TryGetUserId(out var userId) ? userId : null,
                TryGetUserRole(),
                request.Message,
                request.Page,
                request.ClientContext),
            cancellationToken);

        return Ok(ApiResponse.Ok(result, "AI chat response generated"));
    }

    [AllowAnonymous]
    [HttpGet("conversations/{conversationId:guid}")]
    public async Task<IActionResult> GetConversation(Guid conversationId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetAiChatConversationQuery(
                conversationId,
                TryGetUserId(out var userId) ? userId : null),
            cancellationToken);

        return Ok(ApiResponse.Ok(result));
    }

    [AllowAnonymous]
    [HttpPost("conversations/{conversationId:guid}/resolve")]
    public async Task<IActionResult> ResolveConversation(Guid conversationId, CancellationToken cancellationToken)
    {
        await _sender.Send(
            new ResolveAiChatConversationCommand(
                conversationId,
                TryGetUserId(out var userId) ? userId : null),
            cancellationToken);

        return Ok(ApiResponse.Ok("AI chat conversation resolved"));
    }

    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var subject = User.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value;
        return subject is not null && Guid.TryParse(subject, out userId);
    }

    private string? TryGetUserRole()
        => User.Claims.FirstOrDefault(claim => claim.Type == "role")?.Value;
}
