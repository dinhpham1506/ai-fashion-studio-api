# PRD — AI T-Shirt Customization & Virtual Try-On (MVP Learning)

Bản rút gọn chính thức. Mục tiêu: demo end-to-end + học backend system design + Kafka.

## 1. Mục tiêu
Nền tảng cho phép khách: xem áo → tự thiết kế (drag & drop) → lưu design → AI try-on → đặt hàng → thanh toán → theo dõi đơn → feedback. MVP để chứng minh kiến trúc, không phải bán thật.

## 2. Scope
**In scope:** Auth (register/login/refresh/RBAC), Product Catalog, Design Editor, AI Virtual Try-On (async qua Kafka), Order, Payment (PayOS/SePay), Production/Shipping (staff), Feedback, About Us.

**Out of scope:** refund, voucher, chat real-time, multi-store, AI sinh thiết kế, marketplace, mobile native, analytics nâng cao.

## 3. Roles
- **CUSTOMER** — đăng ký, thiết kế, try-on, đặt hàng, thanh toán, theo dõi, feedback.
- **STAFF** — xem đơn đã trả tiền, file in, cập nhật sản xuất/giao hàng, tải hóa đơn, duyệt feedback.
- **ADMIN** — quản lý user/product/inventory/order/payment/About Us/feedback.

## 4. Main flow
Guest → xem About Us/Product/Feedback → Register/Login → Browse → chọn variant → Design Editor → Save Design → Request Try-On (async) → xem kết quả → Create Order → Payment → Order PAID (lock design) → Staff cập nhật production/shipping → Order Completed → Feedback.

## 5. Business rules (rút gọn)
- BR-001 phải login để lưu design.
- BR-002 design phải SAVED trước khi try-on.
- BR-003 AI không sửa design gốc; BR-005 file in lấy từ design, không lấy từ try-on result.
- BR-006 không tạo order nếu variant hết hàng.
- BR-007 payment success → order PAID; BR-008 order PAID → design LOCKED.
- BR-010 chỉ feedback khi order COMPLETED; BR-011 chỉ feedback APPROVED mới public.
- BR-012 chỉ About Us PUBLISHED mới hiển thị.

## 6. Service split
- **Java (java-core-api):** Catalog, Design+Try-On, Order, Feedback.
- **C# (dotnet-platform-api):** Identity, Payment+Invoice, Staff Operation Gateway, Content (About Us).
- **API Gateway (api-gateway):** Spring Cloud Gateway — entry layer, JWT, role guard.

## 7. Kafka events
`OrderCreated`, `PaymentSucceeded`, `PaymentFailed`, `TryOnRequested`, `TryOnCompleted`, `OrderCompleted`.
Topics: `order.events`, `payment.events`, `design.events`, `tryon.events`, `feedback.events`, `notification.events`.

## 8. Non-functional
JWT + RBAC, PostgreSQL, deploy **local hết** bằng Docker Compose. Object storage = MinIO.

## 9. Database
18 bảng / 8 schema trong 1 PostgreSQL. Chi tiết: [DATABASE_DESIGN.md](DATABASE_DESIGN.md).
