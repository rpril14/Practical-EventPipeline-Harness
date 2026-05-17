# Scripts

## Docker Compose

`docker-compose.yml` starts the full infrastructure for local development:

| Service | Image | Port | Purpose |
|---|---|---|---|
| MySQL | mysql:8.0 | 3306 | Transactional store (source of truth) |
| Kafka | confluentinc/cp-kafka:7.9.0 | 9092 | Event streaming |
| Debezium | debezium/connect:3.0.0.Final | 8083 | CDC connector (MySQL binlog → Kafka) |
| Elasticsearch | elasticsearch:8.17.0 | 9200 | Search index |
| ClickHouse | clickhouse/clickhouse-server | 8123 | Analytics store |
| Adminer | adminer | 8080 | MySQL admin UI |
| Kafka-UI | provectuslabs/kafka-ui | 8081 | Kafka topic browser |

```bash
# Start all services
cd scripts && docker compose up -d

# Stop all services
docker compose down
```

**Note:** add `127.0.0.1 kafka` to your hosts file once so the .NET Worker can
reach Kafka by hostname from the host machine.

## Commands

```bash
# Apply database migration
dotnet ef database update \
  --project src/EventPipeline.Data \
  --startup-project src/EventPipeline.Api

# Run all tests
dotnet test

# Start API
dotnet run --project src/EventPipeline.Api

# Start Worker
dotnet run --project src/EventPipeline.Worker
```
