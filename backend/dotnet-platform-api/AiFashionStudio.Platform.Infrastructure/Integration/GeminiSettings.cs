namespace AiFashionStudio.Platform.Infrastructure.Integration;

public class GeminiSettings
{
    public const string SectionName = "Gemini";

    /// <summary>API key lấy tại https://aistudio.google.com/apikey (free tier).</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Model dùng cho generateContent, ví dụ gemini-2.5-flash.</summary>
    public string Model { get; set; } = "gemini-2.5-flash";

    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/";

    /// <summary>Timeout mỗi request (giây) — free tier có thể chậm khi bị rate limit.</summary>
    public int TimeoutSeconds { get; set; } = 15;
}
