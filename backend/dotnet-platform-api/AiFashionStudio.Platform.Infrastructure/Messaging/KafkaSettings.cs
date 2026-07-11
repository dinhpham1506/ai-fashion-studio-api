namespace AiFashionStudio.Platform.Infrastructure.Messaging;

public class KafkaSettings
{
    public const string SectionName = "Kafka";

    /// <summary>Danh sách broker, ví dụ localhost:29092 (external port của docker-compose).</summary>
    public string BootstrapServers { get; set; } = string.Empty;

    /// <summary>Tắt Kafka khi chạy local không có broker — publish sẽ chỉ log warning thay vì lỗi.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Topic C# publish payment events (PaymentSucceeded/PaymentFailed) — Java consume.</summary>
    public string PaymentEventsTopic { get; set; } = "payment.events";

    /// <summary>Topic Java publish order events (OrderCreated/OrderCompleted) — C# consume.</summary>
    public string OrderEventsTopic { get; set; } = "order.events";

    /// <summary>Consumer group của platform API khi consume order.events.</summary>
    public string ConsumerGroupId { get; set; } = "dotnet-platform-api";
}
