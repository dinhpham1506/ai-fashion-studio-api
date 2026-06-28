# AIShopTee3d

MVP learning project for an AI T-shirt customization and virtual try-on platform.

## Local Infra

Start PostgreSQL, Kafka, Kafka UI, and pgAdmin:

```bash
cd infra
cp .env.example .env
docker compose --env-file .env up -d
```

Useful URLs:

- Kafka UI: http://localhost:8085
- pgAdmin: http://localhost:5050
- PostgreSQL: `localhost:5432`
- Kafka external bootstrap: `localhost:29092`

See [infra/README.md](infra/README.md) for Kafka commands and service connection values.
