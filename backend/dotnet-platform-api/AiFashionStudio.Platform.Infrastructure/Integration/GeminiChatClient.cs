using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiFashionStudio.Platform.Infrastructure.Integration;

/// <summary>
/// Calls Gemini generateContent to make AI Chat replies sound natural while staying grounded
/// in data that the tool layer has already fetched and validated.
/// </summary>
public class GeminiChatClient : IGeminiChatClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;
    private readonly ILogger<GeminiChatClient> _logger;

    public GeminiChatClient(HttpClient httpClient, IOptions<GeminiSettings> settings, ILogger<GeminiChatClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string?> GenerateReplyAsync(string systemInstruction, string userPrompt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            return null;
        }

        var requestBody = new GeminiGenerateRequest(
            new GeminiSystemInstruction(new[] { new GeminiPart(systemInstruction) }),
            new[] { new GeminiContent("user", new[] { new GeminiPart(userPrompt) }) },
            new GeminiGenerationConfig(0.4, 512));

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"v1beta/models/{_settings.Model}:generateContent")
        {
            Content = JsonContent.Create(requestBody, options: JsonOptions)
        };
        request.Headers.TryAddWithoutValidation("x-goog-api-key", _settings.ApiKey);

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Gemini API returned {StatusCode}, falling back to template reply: {Body}",
                    (int)response.StatusCode, body);
                return null;
            }

            var result = JsonSerializer.Deserialize<GeminiGenerateResponse>(body, JsonOptions);
            var text = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
            return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        }
        catch (Exception exception) when (exception is HttpRequestException
            || (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested)
            || exception is JsonException)
        {
            _logger.LogWarning(exception, "Gemini API call failed, falling back to template reply");
            return null;
        }
    }

    private sealed record GeminiGenerateRequest(
        [property: JsonPropertyName("system_instruction")] GeminiSystemInstruction SystemInstruction,
        GeminiContent[] Contents,
        [property: JsonPropertyName("generation_config")] GeminiGenerationConfig GenerationConfig);

    private sealed record GeminiSystemInstruction(GeminiPart[] Parts);

    private sealed record GeminiContent(string Role, GeminiPart[] Parts);

    private sealed record GeminiPart(string Text);

    private sealed record GeminiGenerationConfig(double Temperature, int MaxOutputTokens);

    private sealed record GeminiGenerateResponse(GeminiCandidate[]? Candidates);

    private sealed record GeminiCandidate(GeminiContent? Content);
}
