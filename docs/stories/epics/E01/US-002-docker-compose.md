# US-002 Docker Compose Infrastructure

## Status

implemented

## Lane

normal

## Product Contract

A developer can start all seven infrastructure services with a single
`docker compose up -d` and reach each service on its documented port.

## Relevant Product Docs

- `docs/product/overview.md`
- `docs/product/pipeline.md`

## Acceptance Criteria

- `scripts/docker-compose.yml` defines: MySQL 8.0, Kafka (KRaft), Debezium Connect 3.0, Elasticsearch 8, ClickHouse, Adminer, Kafka-UI.
- All services share a bridge network named `pipeline`.
- MySQL starts with `binlog_format=ROW`, `gtid_mode=ON`, `enforce_gtid_consistency=ON`, `server-id=1`.
- Kafka runs in KRaft mode (no Zookeeper) with `KAFKA_AUTO_CREATE_TOPICS_ENABLE=true`.
- Elasticsearch starts with `discovery.type=single-node` and `xpack.security.enabled=false`.
- Volumes defined for `mysql_data`, `es_data`, `clickhouse_data`.
- `docker compose ps` shows all seven services running.
- MySQL binlog format confirmed: `SHOW VARIABLES LIKE 'binlog_format'` returns `ROW`.

## Design Notes

- Commands: `docker compose -f scripts/docker-compose.yml up -d`
- Queries: none
- API: none
- Tables: none
- Domain rules: none
- UI surfaces: Adminer at :8080, Kafka-UI at :8081

## Validation

| Layer | Expected proof |
| --- | --- |
| Unit | none |
| Integration | `docker compose ps` all running; MySQL binlog_format=ROW |
| E2E | none |
| Platform | Docker Compose on local dev machine |
| Release | none |

## Harness Delta

none

## Evidence

- `scripts/docker-compose.yml` created with 7 services on `pipeline` network
- Integration proof pending: run `docker compose up -d` and verify all services running
