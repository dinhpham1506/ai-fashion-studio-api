# ai-fashion-studio Local Infrastructure

This folder contains the shared local infrastructure for the MVP learning version:

- PostgreSQL 16
- Kafka 3.7 in KRaft mode
- Kafka UI
- pgAdmin

## Start

```bash
cd infra
cp .env.example .env
docker compose --env-file .env up -d
```

## Stop

```bash
cd infra
docker compose --env-file .env down
```

## Reset All Local Data

This deletes PostgreSQL and Kafka local volumes.

```bash
cd infra
docker compose --env-file .env down -v
docker compose --env-file .env up -d
```

## Connection Values

PostgreSQL:

```text
Host: localhost
Port: 5432
Database: ai_fashion_studio_db
Username: aifashionstudio
Password: aifashionstudio_password
```

Kafka:

```text
Host bootstrap server: localhost:29092
Docker bootstrap server: kafka:9092
Kafka UI: http://localhost:8085
```

pgAdmin:

```text
URL: http://localhost:5050
Email: admin@aifashionstudio.local
Password: admin123
```

When adding the PostgreSQL server in pgAdmin, use `postgres` as the host because pgAdmin runs inside Docker.

## MVP Topic Map

| Topic | Main Producer | Main Consumer |
| --- | --- | --- |
| `order.events` | Java Order Service | C# Payment Service |
| `payment.events` | C# Payment Service | Java Order Service |
| `design.events` | Java Design/Try-On Service | AI mock worker |
| `tryon.events` | AI mock worker | Java Try-On Service |
| `feedback.events` | Java Feedback Service | optional notification/admin |
| `notification.events` | any backend service | notification worker later |

## MVP Database Schemas

The init scripts create these logical schemas:

- `identity`
- `catalog`
- `design`
- `ai_tryon`
- `ordering`
- `payment`
- `feedback`
- `content`

The scripts also create the 18 MVP tables from the database design document.
