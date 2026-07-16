# Java Core API Structure

This service owns the Java core domains: catalog, design, try-on, and ordering.

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

## Ordering staff APIs

Status: implemented in Java core.

## Cart APIs

Status: implemented in Java core.

- `GET /api/cart`
  - Header: `X-User-Id`.
  - Purpose: get current customer cart.
- `POST /api/cart/items`
  - Header: `X-User-Id`.
  - Body: `productId`, `productVariantId`, `designId`, `quantity`.
  - Purpose: add a saved design/product variant to cart.
- `PATCH /api/cart/items/{itemId}`
  - Header: `X-User-Id`.
  - Body: `quantity`.
  - Purpose: update cart item quantity.
- `DELETE /api/cart/items/{itemId}`
  - Header: `X-User-Id`.
  - Purpose: remove one cart item.
- `DELETE /api/cart`
  - Header: `X-User-Id`.
  - Purpose: clear cart.
- `POST /api/cart/checkout`
  - Header: `X-User-Id`.
  - Body: receiver info.
  - Purpose: create an order from all cart items, reserve inventory, then clear cart.

- `GET /api/staff/orders`
  - Role: `STAFF` or `ADMIN` via `X-User-Role`.
  - Query: `page` default `1`, `pageSize` default `10`.
  - Response: `PagedOrderResponse`, sorted by newest order first.
  - Purpose: staff order list table.
- `GET /api/staff/orders/{orderId}/print-info`
  - Role: `STAFF` or `ADMIN` via `X-User-Role`.
  - Response: `OrderPrintInfoResponse` with receiver info, order items, and design `previewImageUrl`/`printFileUrl`.
  - Purpose: print preparation screen/action.
- `PATCH /api/staff/orders/{orderId}/status`
  - Role: `STAFF` or `ADMIN` via `X-User-Role`.
  - Body: `UpdateOrderStatusRequest`.
  - Current allowed domain transitions: `PAID -> IN_PRODUCTION`, `IN_PRODUCTION -> SHIPPING`, `SHIPPING -> COMPLETED`, `PAID -> CANCELLED`.
