# System Architecture — AI T-Shirt Customization Platform

Tài liệu kiến trúc chính thức (source of truth). Sơ đồ C4 Container ở cuối file (PlantUML).

## Quyết định đã chốt

| Hạng mục | Quyết định |
|---|---|
| Repo | Monorepo, clone full, mỗi người mở subfolder của mình |
| Database | **1 PostgreSQL chung** — 18 bảng / 8 schema (KHÔNG tách DB per-service) |
| API entry | **API Gateway riêng** — Spring Cloud Gateway (Java), routing + JWT + role guard |
| Object storage | **MinIO** (S3-compatible) — product images, design preview, print files, try-on results, invoice PDFs |
| AI Try-On | Gọi **API ngoài** trực tiếp trong Java (tryon module), không tách worker riêng |
| Payment | **Thật** — PayOS / SePay + webhook về C# Payment Service |
| Messaging | Kafka (event bus) — do team tự cấu hình producer/consumer |
| Deploy | **Local hết** bằng Docker Compose |

## Phân chia service & ownership

**Java (`backend/java-core-api`)** — core domain
- Catalog: products, product_variants, product_images, product_inventory
- Design + Try-On: designs, design_layers, tryon_requests, tryon_results
- Order: orders, order_items, order_status_history

**C# (`backend/dotnet-platform-api`)** — platform & operation
- Identity: users, roles, user_roles, refresh_tokens
- Payment & Invoice: payments
- Feedback: feedbacks
- Staff Operation Gateway + Content: about_us_contents

**Gateway (`backend/api-gateway`)** — Spring Cloud Gateway, điểm vào duy nhất cho frontend.

## Cổng (local)

| Thành phần | Host port |
|---|---|
| api-gateway (FE gọi vào đây) | 8080 |
| java-core-api | 8081 |
| dotnet-platform-api | 8082 |
| postgres | 5432 |
| kafka (external) | 29092 |
| minio API / console | 9000 / 9001 |
| pgadmin / kafka-ui | 5050 / 8085 |

## Luồng chính
1. FE → **api-gateway** (JWT + role) → định tuyến tới Java hoặc C#.
2. Order tạo ở Java → publish `OrderCreated` → C# Payment consume.
3. C# Payment tạo checkout PayOS/SePay → webhook về C# → publish `PaymentSucceeded`.
4. Java Order consume `PaymentSucceeded` → order PAID + lock design + status history.
5. Try-On: Java gọi API AI ngoài, lưu `tryon_results` (mock/real).
6. Staff thao tác qua C# Operation Gateway → gọi Java Order API.

---

## C4 Container Diagram (PlantUML)

```plantuml
@startuml
title AI T-Shirt Customization Platform - Container Diagram

skinparam backgroundColor white
skinparam shadowing false
skinparam roundcorner 15
skinparam packageStyle rectangle

actor "Customer" as Customer
actor "Staff / Admin" as Staff

rectangle "AI T-Shirt Platform\nSystem Boundary" as System {

  rectangle "Web Frontend\nReact + TypeScript\nTailwind CSS\nFabric.js Basic\n2 FE Fresher" as FE #E0F2FE

  rectangle "API Entry Layer\nREST API\nJWT Protected APIs\nRole Guard" as API #F8FAFC

  package "C# Platform Services\nFresher Scope" {

    rectangle "Identity Service\nC# .NET 8\n\nRegister/Login\nJWT\nRefresh Token\nRBAC\n\nOwns:\nusers, roles,\nuser_roles,\nrefresh_tokens" as Identity #FEF3C7

    rectangle "Payment & Invoice Service\nC# .NET 8\n\nPayOS / SePay\nWebhook\nPayment Status\nInvoice PDF\n\nOwns:\npayments" as Payment #FEF3C7

    rectangle "Staff Operation Gateway\nC# .NET 8\n\nValidate Staff/Admin\nPrint Info\nAbout Us\nCall Java APIs\n\nOwns:\nabout_us_contents" as StaffGateway #FEF3C7

    rectangle "Feedback Service\nC# .NET 8\n\nSubmit Feedback\nModerate Feedback\nPublic Feedback\n\nOwns:\nfeedbacks" as Feedback #FEF3C7
  }

  package "Java Core Domain Services\nJunior Core Scope" {

    rectangle "Catalog Service\nJava Spring Boot\n\nProduct\nVariant\nImage\nInventory\n\nOwns:\nproducts,\nproduct_variants,\nproduct_images,\nproduct_inventory" as Catalog #DCFCE7

    rectangle "Design Try-On Service\nJava Spring Boot\n\nSave Design\nSave Layers\nTry-On Request\nTry-On Result\nLock Design\n\nOwns:\ndesigns,\ndesign_layers,\ntryon_requests,\ntryon_results" as Design #DCFCE7

    rectangle "Order Service\nJava Spring Boot\n\nCreate Order\nOrder Items\nStatus Lifecycle\nConsume PaymentSucceeded\n\nOwns:\norders,\norder_items,\norder_status_history" as Order #DCFCE7

  }

  queue "Kafka Event Bus\n\nOrderCreated\nPaymentSucceeded\nPaymentFailed\nTryOnRequested\nTryOnCompleted\nOrderCompleted" as Kafka #F3E8FF

  rectangle "AI Try-On Worker\n\nMock AI in MVP\nConsume TryOnRequested\nProduce TryOnCompleted" as AI #E0E7FF
}

database "PostgreSQL Database\n18 Tables\n\nidentity: users, roles, user_roles, refresh_tokens\ncatalog: products, variants, images, inventory\ndesign: designs, design_layers\nai_tryon: requests, results\nordering: orders, items, history\npayment: payments\nfeedback: feedbacks\ncontent: about_us_contents" as DB #E5E7EB

database "Object Storage\n\nProduct images\nDesign preview\nPrint files\nTry-On results\nInvoice PDFs" as Storage #E5E7EB

cloud "PayOS / SePay\nExternal Payment Provider\n\nCheckout link\nQR payment\nBank transfer\nWebhook callback" as Provider #FFE4E6

Customer --> FE : Uses web app
Staff --> FE : Uses staff/admin UI

FE --> API : REST API + JWT

API --> Identity : Auth APIs
API --> Payment : Payment APIs
API --> StaffGateway : Staff/Admin APIs
API --> Catalog : Product APIs
API --> Design : Design / Try-On APIs
API --> Order : Order APIs
API --> Feedback : Feedback APIs

Identity --> DB : Read/Write identity tables
Payment --> DB : Read/Write payments
StaffGateway --> DB : Read/Write content

Catalog --> DB : Read/Write catalog tables
Design --> DB : Read/Write design + tryon tables
Order --> DB : Read/Write order tables
Feedback --> DB : Read/Write feedbacks

Catalog --> Storage : Product images
Design --> Storage : Design preview + print file
AI --> Storage : Try-On result image
Payment --> Storage : Invoice PDF

Payment --> Provider : Create payment link / QR
Provider -[#red,dashed]-> Payment : Payment webhook

Order ..> Kafka : Publish OrderCreated
Kafka ..> Payment : Consume OrderCreated

Payment ..> Kafka : Publish PaymentSucceeded
Kafka ..> Order : Consume PaymentSucceeded

Design ..> Kafka : Publish TryOnRequested
Kafka ..> AI : Consume TryOnRequested

AI ..> Kafka : Publish TryOnCompleted
Kafka ..> Design : Consume TryOnCompleted

Order ..> Kafka : Publish OrderCompleted
Kafka ..> Feedback : Enable feedback flow

StaffGateway --> Order : Update order status
StaffGateway --> Design : Get print file info
StaffGateway --> Feedback : Moderate feedback

note bottom of System
SA Notes:
1. Java owns catalog, design/try-on and ordering core.
2. C# owns identity, payment, invoice, feedback and staff gateway.
3. Payment uses PayOS or SePay.
4. Payment webhook goes to C# Payment Service.
5. Java updates order after PaymentSucceeded event.
6. Try-On runs async through Kafka.
end note

@enduml
```
