# Database Design — ai-fashion-studio

**1 PostgreSQL** (`ai_fashion_studio_db`), **18 bảng** chia **8 schema** theo service boundary.
DDL thật (source of truth): [infra/postgres/init/](../infra/postgres/init/).

## Quy ước chung
- PK `UUID` mặc định `gen_random_uuid()` (pgcrypto).
- Mọi timestamp là `TIMESTAMPTZ` (UTC).
- Trigger `set_updated_at()` tự cập nhật `updated_at`.
- Index thủ công cho cột FK + cột `status` hay query.
- **Không có FK xuyên ranh giới Java↔C#** — tham chiếu qua UUID rời, đồng bộ bằng API/Kafka.

## Ownership

| Schema | Bảng | Owner |
|---|---|---|
| identity | users, roles, user_roles, refresh_tokens | C# |
| payment | payments (+ invoice_number, invoice_pdf_url) | C# |
| content | about_us_contents | C# |
| catalog | products, product_variants, product_images, product_inventory | Java |
| design | designs, design_layers | Java |
| ai_tryon | tryon_requests, tryon_results | Java |
| ordering | orders, order_items, order_status_history | Java |
| feedback | feedbacks | Java |

## Boundary rule
Service nào sở hữu bảng thì chỉ service đó được ghi. Ví dụ: C# Payment ghi `payments` rồi publish `PaymentSucceeded`; Java Order consume event và tự cập nhật `orders` — C# không ghi thẳng `orders`.

## Điểm cần lưu ý (đã ghi nhận, làm khi tới)
- Idempotency cho Kafka consumer (bảng `processed_events`) — tránh xử lý trùng.
- Outbox pattern cho dual-write DB + Kafka.
- `orders.payment_status` là projection; source of truth là `payments`.
- Inventory: reserve khi tạo order, sold khi PAID, release khi hủy — cần `SELECT ... FOR UPDATE`.

Mọi cột chi tiết xem trực tiếp trong file DDL.
