# ai-fashion-studio

MVP learning project — AI T-shirt customization & virtual try-on platform.
Monorepo, **deploy local hết** bằng Docker Compose. Stack đều free + open-source.

> Kiến trúc & quyết định: [docs/SYSTEM_ARCHITECTURE.md](docs/SYSTEM_ARCHITECTURE.md) · PRD: [docs/PRD.md](docs/PRD.md) · DB: [docs/DATABASE_DESIGN.md](docs/DATABASE_DESIGN.md)

## Clone

Đây là **monorepo** — clone **full** (không clone lẻ folder). Mỗi người mở subfolder của mình trong IDE.

```bash
git clone <repo-url>
cd ai-fashion-studio
cp .env.example .env
```

## Cấu trúc

```
backend/
  api-gateway/           Spring Cloud Gateway — entry layer (JWT, routing)   [Java]
  java-core-api/         catalog, design, tryon, ordering, feedback          [Java]
  dotnet-platform-api/   identity, payment+invoice, content, staff gateway   [C#]
frontend/                customer-web, admin-staff-web (React)
infra/                   docker-compose: postgres, kafka, minio, UIs
contracts/               kafka schemas + openapi
docs/                    PRD, database design, system architecture
```

## Chạy

**Cách 1 — chạy hết (gateway + 2 backend + infra):**
```bash
docker compose up --build
```

**Cách 2 — chỉ infra (dev backend bằng IDE):**
```bash
cd infra && docker compose --env-file .env up -d
```

## Cổng (local)

| URL | Service |
|---|---|
| http://localhost:8080 | api-gateway (FE gọi vào đây) |
| http://localhost:8081 | java-core-api |
| http://localhost:8082 | dotnet-platform-api |
| http://localhost:5050 | pgAdmin |
| http://localhost:8085 | Kafka UI |
| http://localhost:9001 | MinIO console |
| localhost:5432 | PostgreSQL |
| localhost:29092 | Kafka (external bootstrap) |

Chi tiết Kafka/DB: [infra/README.md](infra/README.md).

## Phân chia việc
- **Java dev** → `backend/java-core-api` + `backend/api-gateway`
- **C# dev** → `backend/dotnet-platform-api`
- **Frontend** → `frontend/`

Kafka producer/consumer và logic nghiệp vụ do team tự code.
