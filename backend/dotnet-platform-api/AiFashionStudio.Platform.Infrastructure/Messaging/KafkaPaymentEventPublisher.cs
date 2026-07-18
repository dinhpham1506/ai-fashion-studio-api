using System.Text.Json;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using AiFashionStudio.Platform.Application.Common.Models;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiFashionStudio.Platform.Infrastructure.Messaging;

/// <summary>
/// Publish payment events lên topic payment.events theo envelope chuẩn
/// {eventId, eventType, occurredAt, data} để Java Order Service consume.
/// Kafka lỗi/không bật thì chỉ log — không được làm hỏng flow thanh toán đã PAID.
/// </summary>
public sealed class KafkaPaymentEventPublisher : IPaymentEventPublisher, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly KafkaSettings _settings;
    private readonly ILogger<KafkaPaymentEventPublisher> _logger;
    private readonly Lazy<IProducer<string, string>> _producer;

    public KafkaPaymentEventPublisher(IOptions<KafkaSettings> options, ILogger<KafkaPaymentEventPublisher> logger)
    {
        _settings = options.Value;
        _logger = logger;
        _producer = new Lazy<IProducer<string, string>>(() =>
            new ProducerBuilder<string, string>(new ProducerConfig
            {
                BootstrapServers = _settings.BootstrapServers,
                // Chờ tối đa 5s khi broker chưa sẵn sàng để API không bị treo lâu
                MessageTimeoutMs = 5000,
                Acks = Acks.All
            }).Build());
    }

    public Task PublishPaymentSucceededAsync(PaymentSucceededEventData data, CancellationToken cancellationToken) =>
        PublishAsync("PaymentSucceeded", data.OrderId?.ToString() ?? data.OrderCode.ToString(), data, cancellationToken);

    public Task PublishPaymentFailedAsync(PaymentFailedEventData data, CancellationToken cancellationToken) =>
        PublishAsync("PaymentFailed", data.OrderId?.ToString() ?? data.OrderCode.ToString(), data, cancellationToken);

    private async Task PublishAsync<T>(string eventType, string key, T data, CancellationToken cancellationToken)
    {
        if (!_settings.Enabled || string.IsNullOrWhiteSpace(_settings.BootstrapServers))
        {
            _logger.LogWarning("Kafka disabled — skipped publishing {EventType} (key {Key})", eventType, key);
            return;
        }

        var envelope = IntegrationEventEnvelope<T>.Create(eventType, data);
        var payload = JsonSerializer.Serialize(envelope, JsonOptions);

        try
        {
            var result = await _producer.Value.ProduceAsync(
                _settings.PaymentEventsTopic,
                new Message<string, string> { Key = key, Value = payload },
                cancellationToken);

            _logger.LogInformation(
                "Published {EventType} to {Topic} [{Partition}] offset {Offset} (eventId {EventId})",
                eventType, result.Topic, result.Partition.Value, result.Offset.Value, envelope.EventId);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Payment đã ghi DB thành công — event lỗi thì log để xử lý tay/retry sau,
            // không throw ngược làm webhook trả lỗi khiến provider retry vô hạn.
            _logger.LogError(exception, "Failed to publish {EventType} for key {Key}", eventType, key);
        }
    }

    public void Dispose()
    {
        if (_producer.IsValueCreated)
        {
            _producer.Value.Flush(TimeSpan.FromSeconds(5));
            _producer.Value.Dispose();
        }
    }
}
