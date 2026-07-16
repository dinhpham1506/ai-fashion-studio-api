namespace AiFashionStudio.Platform.Infrastructure.Integration;

public class JavaCoreApiSettings
{
    public const string SectionName = "JavaCoreApi";

    /// <summary>Base URL của java-core-api, ví dụ http://localhost:8081 (gọi thẳng service, không đi qua gateway).</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Timeout mỗi request (giây) — gateway không nên treo lâu khi Java chậm.</summary>
    public int TimeoutSeconds { get; set; } = 10;
}
