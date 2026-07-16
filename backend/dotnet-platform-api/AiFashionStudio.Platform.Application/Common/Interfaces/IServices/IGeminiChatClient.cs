namespace AiFashionStudio.Platform.Application.Common.Interfaces.IServices;

/// <summary>
/// Gọi Gemini API (LLM free tier) để diễn giải dữ liệu đã được tool layer lấy sẵn
/// (sản phẩm, size, đơn hàng...) thành câu trả lời tiếng Việt tự nhiên cho AI Chat.
/// Không dùng để tự truy vấn dữ liệu hay quyết định nghiệp vụ — chỉ sinh văn bản.
/// </summary>
public interface IGeminiChatClient
{
    /// <summary>
    /// Trả về câu trả lời do Gemini sinh ra, hoặc null nếu gọi thất bại (rate limit,
    /// timeout, thiếu API key...). Caller nên fallback sang câu trả lời template có sẵn khi nhận null.
    /// </summary>
    Task<string?> GenerateReplyAsync(string systemInstruction, string userPrompt, CancellationToken cancellationToken);
}
