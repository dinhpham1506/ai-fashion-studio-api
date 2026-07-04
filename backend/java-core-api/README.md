# Java Core API Structure

This service owns the Java core domains: catalog, design, try-on, ordering, and feedback.

Package convention per domain:

- `api/controller`: REST controllers.
- `api/dto`: HTTP request/response DTOs.
- `application/service`: use-case services and transaction boundaries.
- `application/command`: write-side command objects.
- `application/query`: read-side query objects.
- `application/dto`: internal application DTOs.
- `application/mapper`: mapping between API/application/domain objects.
- `domain/model`: domain models and enums.
- `domain/event`: domain events used by Kafka or internal flows.
- `domain/repository`: repository ports/interfaces owned by domain/application.
- `domain/service`: domain rules that do not naturally belong to one model.
- `infrastructure/persistence/entity`: JPA entities mapped to PostgreSQL tables.
- `infrastructure/persistence/repository`: Spring Data JPA repositories and adapters.
- `infrastructure/persistence/mapper`: mapping between JPA entities and domain models.
- `infrastructure/messaging/producer`: Kafka producers.
- `infrastructure/messaging/consumer`: Kafka consumers.
- `infrastructure/client`: external service clients.

Shared code belongs under `shared`. Cross-service calls to C# belong under `integration/csharp`.
