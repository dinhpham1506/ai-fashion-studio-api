# Kafka Event Contracts

These schemas define the first shared event payloads between Java, C#, and the AI mock worker.

Recommended topic usage:

| Event | Topic | Producer | Consumer |
| --- | --- | --- | --- |
| `OrderCreated` | `order.events` | Java Order Service | C# Payment Service |
| `PaymentSucceeded` | `payment.events` | C# Payment Service | Java Order Service |
| `TryOnRequested` | `design.events` | Java Try-On Service | AI mock worker |
| `TryOnCompleted` | `tryon.events` | AI mock worker | Java Try-On Service |

For MVP, keep payloads small and stable. If a service needs more detail, it should call the owning service API using the IDs from the event instead of putting full aggregate data into Kafka.
