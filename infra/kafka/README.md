# Kafka Local Commands

Bootstrap servers:

- From host machine: `localhost:29092`
- From Docker network: `kafka:9092`

List topics:

```bash
docker exec -it aishoptee3d-kafka /opt/bitnami/kafka/bin/kafka-topics.sh --bootstrap-server localhost:9092 --list
```

Create a topic manually:

```bash
docker exec -it aishoptee3d-kafka /opt/bitnami/kafka/bin/kafka-topics.sh \
  --bootstrap-server localhost:9092 \
  --create \
  --if-not-exists \
  --topic order.events \
  --partitions 3 \
  --replication-factor 1
```

Produce a test message:

```bash
docker exec -it aishoptee3d-kafka /opt/bitnami/kafka/bin/kafka-console-producer.sh \
  --bootstrap-server localhost:9092 \
  --topic order.events
```

Example message:

```json
{"eventId":"demo-1","eventType":"OrderCreated","orderId":"00000000-0000-0000-0000-000000000001"}
```

Consume messages:

```bash
docker exec -it aishoptee3d-kafka /opt/bitnami/kafka/bin/kafka-console-consumer.sh \
  --bootstrap-server localhost:9092 \
  --topic order.events \
  --from-beginning
```

Recommended event ownership:

- `order.events`: Java Order Service produces `OrderCreated`, `OrderCompleted`.
- `payment.events`: C# Payment Service produces `PaymentSucceeded`, `PaymentFailed`.
- `design.events`: Java Design/Try-On flow produces `TryOnRequested`.
- `tryon.events`: AI mock worker produces `TryOnCompleted`, `TryOnFailed`.
- `feedback.events`: Java Feedback Service can produce feedback moderation events later.
- `notification.events`: reserved for notification fan-out later.
