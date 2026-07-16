# Dotnet API Test Guide

Tài liệu này dùng để test các API hiện có trong `backend/dotnet-platform-api`.

## Base URL

- Chạy qua Docker Compose: `http://localhost:8082`
- Chạy bằng Visual Studio / `dotnet run`:
  - `https://localhost:57591`
  - `http://localhost:57592`
- Swagger UI:
  - `http://localhost:8082/swagger`
  - `https://localhost:57591/swagger`

## Health Check

- `GET /health`
- `GET /api/ping`

## Cơ chế auth

- API dùng JWT Bearer cho access token.
- `POST /api/auth/login` trả về `accessToken` trong response body.
- `refresh_token` được lưu bằng `HttpOnly cookie`, path `/api/auth`.
- Khi test bằng Postman:
  - Gắn header `Authorization: Bearer <accessToken>` cho API cần đăng nhập.
  - Giữ cookie sau khi gọi `login` nếu muốn test `refresh-token` và `logout`.

## Format response chung

```json
{
  "success": true,
  "message": "Success",
  "data": {},
  "errors": null,
  "meta": {
    "requestId": "string",
    "timestamp": "2026-07-11T00:00:00Z"
  }
}
```

## 1. Auth APIs

### `POST /api/auth/register`

- Auth: Không cần
- Content-Type: `application/json`
- Body:

```json
{
  "email": "user1@example.com",
  "password": "12345678",
  "fullName": "Nguyen Van A",
  "phone": "0901234567"
}
```

- Validate chính:
  - `email` bắt buộc, đúng format
  - `password` tối thiểu 8 ký tự
  - `fullName` bắt buộc

### `POST /api/auth/login`

- Auth: Không cần
- Content-Type: `application/json`
- Body:

```json
{
  "email": "user1@example.com",
  "password": "12345678"
}
```

- Kết quả mong đợi:
  - `data.accessToken`
  - `data.expiresIn`
  - `data.user`
  - cookie `refresh_token`

### `POST /api/auth/refresh-token`

- Auth: Không cần header Bearer
- Yêu cầu: phải có cookie `refresh_token`
- Body: không cần

### `POST /api/auth/logout`

- Auth: Không cần header Bearer
- Yêu cầu: có hoặc không có cookie `refresh_token` đều gọi được

### `GET /api/auth/me`

- Auth: Bearer token
- Body: không cần

### `POST /api/auth/forgot-password`

- Auth: Không cần
- Content-Type: `application/json`
- Body:

```json
{
  "email": "user1@example.com"
}
```

### `POST /api/auth/verify-reset-otp`

- Auth: Không cần
- Content-Type: `application/json`
- Body:

```json
{
  "email": "user1@example.com",
  "otp": "123456"
}
```

- Kết quả mong đợi:
  - `data.resetToken`
  - `data.expiresIn`

### `POST /api/auth/reset-password`

- Auth: Không cần
- Content-Type: `application/json`
- Body:

```json
{
  "resetToken": "paste-reset-token-here",
  "newPassword": "newpass123"
}
```

- Validate chính:
  - `resetToken` bắt buộc
  - `newPassword` tối thiểu 6 ký tự

## 2. User Profile APIs

Tất cả API dưới đây cần Bearer token.

### `GET /api/users/me/profile`

- Auth: Bearer token

### `PATCH /api/users/me/profile`

- Content-Type: `application/json`
- Body:

```json
{
  "fullName": "Nguyen Van A Updated",
  "phone": "0988888888"
}
```

- Validate chính:
  - `fullName` bắt buộc
  - `fullName` tối đa 255 ký tự
  - `phone` tối đa 20 ký tự

### `POST /api/users/me/avatar`

- Content-Type: `multipart/form-data`
- Form-data:
  - `file`: chọn file ảnh

- Validate chính:
  - file bắt buộc
  - tối đa 5MB
  - chỉ nhận `image/jpeg`, `image/png`, `image/webp`

## 3. Payment APIs

### `POST /api/payments`

- Auth: Bearer token
- Content-Type: `application/json`
- Body:

```json
{
  "amount": 10000,
  "description": "Thanh toan don hang test"
}
```

- Validate chính:
  - `amount` > 0
  - `description` bắt buộc
  - `description` tối đa 256 ký tự

### `GET /api/payments/{paymentId}`

- Auth: Bearer token
- Ví dụ: `GET /api/payments/00000000-0000-0000-0000-000000000001`

### `GET /api/payments/order/{orderId}`

- Auth: Bearer token
- Ví dụ: `GET /api/payments/order/00000000-0000-0000-0000-000000000002`

### `POST /api/payments/{orderCode}/cancel`

- Auth: Bearer token
- Ví dụ: `POST /api/payments/123456/cancel`

### `POST /api/payments/webhook`

- Auth: Không cần
- Content-Type: raw body
- Ghi chú:
  - API này dành cho PayOS/webhook thật.
  - Nếu body/signature không hợp lệ sẽ trả `400`.
  - Thường chỉ cần smoke test route khi đã có payload hợp lệ từ phía payment provider.

## 4. Invoice APIs

Tất cả API dưới đây cần Bearer token.

Người dùng thường chỉ xem invoice của chính mình. `STAFF` hoặc `ADMIN` có thể xem rộng hơn.

### `GET /api/invoices/{invoiceId}`

- Ví dụ: `GET /api/invoices/11111111-1111-1111-1111-111111111111`

### `GET /api/invoices/order/{orderId}`

- Ví dụ: `GET /api/invoices/order/22222222-2222-2222-2222-222222222222`

### `GET /api/invoices/{invoiceId}/items`

### `GET /api/invoices/{invoiceId}/pdf`

- Ghi chú:
  - API hiện trả object chứa `invoiceNumber` và `pdfUrl`, không phải stream PDF trực tiếp.

## 5. Feedback APIs

### `POST /api/feedbacks`

- Auth: Bearer token
- Content-Type: `multipart/form-data`
- Form-data:
  - `orderId`: GUID
  - `productId`: GUID
  - `rating`: số từ 1 đến 5
  - `comment`: text, optional
  - `image`: optional

- Ví dụ:
  - `orderId = 33333333-3333-3333-3333-333333333333`
  - `productId = 44444444-4444-4444-4444-444444444444`
  - `rating = 5`
  - `comment = San pham dep`

- Validate chính:
  - `orderId` bắt buộc
  - `productId` bắt buộc
  - `rating` từ 1 đến 5
  - `comment` tối đa 1000 ký tự
  - `image` tối đa 5MB
  - `image` chỉ nhận JPG, PNG, WEBP

### `GET /api/feedbacks/public`

- Auth: Không cần
- Query params:
  - `productId`: optional GUID
  - `page`: mặc định `1`, phải > 0
  - `pageSize`: mặc định `10`, từ `1` đến `100`

- Ví dụ:
  - `/api/feedbacks/public?page=1&pageSize=10`
  - `/api/feedbacks/public?productId=44444444-4444-4444-4444-444444444444&page=1&pageSize=10`

### `GET /api/admin/feedbacks`

- Auth: Bearer token
- Role: `STAFF` hoặc `ADMIN`
- Query params:
  - `status`: optional, một trong `PENDING`, `APPROVED`, `HIDDEN`, `REJECTED`
  - `productId`: optional GUID
  - `page`: mặc định `1`
  - `pageSize`: mặc định `20`, từ `1` đến `100`

### `PATCH /api/admin/feedbacks/{feedbackId}/moderation`

- Auth: Bearer token
- Role: `STAFF` hoặc `ADMIN`
- Content-Type: `application/json`
- Body:

```json
{
  "status": "APPROVED"
}
```

- Giá trị hợp lệ của `status`:
  - `APPROVED`
  - `HIDDEN`
  - `REJECTED`

## 6. About Us APIs

### `GET /api/about-us`

- Auth: Không cần

### `PUT /api/admin/about-us/{sectionKey}`

- Auth: Bearer token
- Role: `STAFF` hoặc `ADMIN`
- Content-Type: `application/json`
- Ví dụ path:
  - `PUT /api/admin/about-us/hero`

- Body:

```json
{
  "title": "About AI Fashion Studio",
  "content": "Noi dung gioi thieu",
  "imageUrl": "https://example.com/about.jpg",
  "status": "PUBLISHED"
}
```

- Validate chính:
  - `sectionKey` bắt buộc, tối đa 100 ký tự
  - `title` bắt buộc, tối đa 255 ký tự
  - `content` bắt buộc
  - `status` chỉ nhận `DRAFT` hoặc `PUBLISHED`

## 7. Thứ tự test nhanh đề xuất

1. `POST /api/auth/register`
2. `POST /api/auth/login`
3. Copy `accessToken`
4. `GET /api/auth/me`
5. `GET /api/users/me/profile`
6. `PATCH /api/users/me/profile`
7. `POST /api/users/me/avatar`
8. `POST /api/payments`
9. `GET /api/payments/{paymentId}`
10. Nếu có dữ liệu sẵn thì test invoice / feedback / about-us

## 8. Lưu ý khi test

- Swagger đã bật ở mọi environment, nên có thể test nhanh ở `/swagger`.
- Một số API cần dữ liệu thật trong DB:
  - invoice cần `invoiceId` hoặc `orderId` có tồn tại
  - feedback cần `orderId` và `productId` hợp lệ
  - admin API cần user có role `STAFF` hoặc `ADMIN`
- `POST /api/auth/refresh-token` và `POST /api/auth/logout` phụ thuộc vào cookie hơn là body.
- Nếu chạy local không qua HTTPS, cookie `refresh_token` có thể bị ảnh hưởng vì đang cấu hình `Secure = true`.
