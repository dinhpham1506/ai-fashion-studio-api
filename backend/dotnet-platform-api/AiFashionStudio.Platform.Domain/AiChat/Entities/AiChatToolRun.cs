using AiFashionStudio.Platform.Domain.Common;

namespace AiFashionStudio.Platform.Domain.AiChat.Entities;

public class AiChatToolRun : BaseEntity
{
    public Guid ConversationId { get; private set; }
    public string ToolName { get; private set; } = default!;
    public string? InputJson { get; private set; }
    public string? OutputSummaryJson { get; private set; }
    public string Status { get; private set; } = default!;
    public string? ErrorMessage { get; private set; }

    private AiChatToolRun()
    {
    }

    public static AiChatToolRun Succeeded(
        Guid conversationId,
        string toolName,
        string? inputJson,
        string? outputSummaryJson)
    {
        return new AiChatToolRun
        {
            ConversationId = conversationId,
            ToolName = toolName.Trim(),
            InputJson = inputJson,
            OutputSummaryJson = outputSummaryJson,
            Status = "SUCCEEDED"
        };
    }

    public static AiChatToolRun Failed(
        Guid conversationId,
        string toolName,
        string? inputJson,
        string errorMessage)
    {
        return new AiChatToolRun
        {
            ConversationId = conversationId,
            ToolName = toolName.Trim(),
            InputJson = inputJson,
            Status = "FAILED",
            ErrorMessage = errorMessage
        };
    }
}

