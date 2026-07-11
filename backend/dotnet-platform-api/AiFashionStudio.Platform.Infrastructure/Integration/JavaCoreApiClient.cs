using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using Microsoft.Extensions.Logging;

namespace AiFashionStudio.Platform.Infrastructure.Integration;

/// <summary>
/// Gọi internal API của java-core-api qua HTTP.
/// Java trả response theo envelope chuẩn {success, message, data, errors, meta} —
/// client này mở envelope và ném AppException tương ứng (404/409/422/502/503).
/// </summary>
public class JavaCoreApiClient : IJavaCoreApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<JavaCoreApiClient> _logger;

    public JavaCoreApiClient(HttpClient httpClient, ILogger<JavaCoreApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<StaffOrderListResponse> GetStaffOrdersAsync(
        string? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = $"?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(status))
        {
            query += $"&status={Uri.EscapeDataString(status)}";
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/internal/orders{query}");
        return await SendAsync<StaffOrderListResponse>(request, cancellationToken);
    }

    public async Task<PrintInfoResponse> GetOrderPrintInfoAsync(Guid orderId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/internal/orders/{orderId}/print-info");
        return await SendAsync<PrintInfoResponse>(request, cancellationToken);
    }

    public async Task<UpdateOrderStatusResponse> UpdateOrderStatusAsync(
        Guid orderId, string toStatus, string? note, Guid changedBy, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/internal/orders/{orderId}/status")
        {
            Content = JsonContent.Create(new { toStatus, note, changedBy }, options: JsonOptions)
        };
        return await SendAsync<UpdateOrderStatusResponse>(request, cancellationToken);
    }

    private async Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException
            || (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            // Java down hoặc timeout — không phải lỗi của phía gọi API này
            _logger.LogError(exception, "Java Core API unreachable: {Url}", request.RequestUri);
            throw new ServiceUnavailableException(
                "ORDER_SERVICE_UNAVAILABLE", "Order service is temporarily unavailable");
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            // Cố gắng giữ nguyên error code/message nghiệp vụ mà Java trả về
            var (code, message) = ExtractError(body);

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    throw new NotFoundException(code ?? "ORDER_NOT_FOUND", message ?? "Order not found");
                case HttpStatusCode.Conflict:
                    throw new ConflictException(
                        code ?? "INVALID_ORDER_STATUS_TRANSITION", message ?? "Invalid order status transition");
                case HttpStatusCode.BadRequest:
                case HttpStatusCode.UnprocessableEntity:
                    throw new AppValidationException(
                        "toStatus", code ?? "ORDER_BUSINESS_RULE_FAILED", message ?? "Order business rule failed");
                default:
                    _logger.LogError(
                        "Java Core API returned {StatusCode} for {Url}: {Body}",
                        (int)response.StatusCode, request.RequestUri, body);
                    throw new BadGatewayException(
                        "ORDER_SERVICE_ERROR", "Order service returned an unexpected error");
            }
        }

        JavaApiEnvelope<T>? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<JavaApiEnvelope<T>>(body, JsonOptions);
        }
        catch (JsonException exception)
        {
            _logger.LogError(exception, "Cannot parse Java Core API response from {Url}", request.RequestUri);
            throw new BadGatewayException(
                "ORDER_SERVICE_INVALID_RESPONSE", "Order service returned an invalid response");
        }

        if (envelope is null || !envelope.Success || envelope.Data is null)
        {
            throw new BadGatewayException(
                "ORDER_SERVICE_INVALID_RESPONSE", envelope?.Message ?? "Order service returned an invalid response");
        }

        return envelope.Data;
    }

    private static (string? Code, string? Message) ExtractError(string body)
    {
        try
        {
            var envelope = JsonSerializer.Deserialize<JavaApiEnvelope<JsonElement>>(body, JsonOptions);
            var firstError = envelope?.Errors?.FirstOrDefault();
            return (firstError?.Code, firstError?.Message ?? envelope?.Message);
        }
        catch (JsonException)
        {
            return (null, null);
        }
    }

    /// <summary>Envelope chuẩn mà mọi API bên Java trả về.</summary>
    private sealed record JavaApiEnvelope<T>(bool Success, string? Message, T? Data, List<JavaApiError>? Errors);

    private sealed record JavaApiError(string? Code, string? Message, string? Field);
}
