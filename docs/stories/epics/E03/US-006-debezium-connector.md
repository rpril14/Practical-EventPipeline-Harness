# US-006 Debezium Connector Registration

## Status

planned

## Lane

high-risk

## Product Contract

A Debezium MySQL connector watches the `orders_db.orders` table and publishes CDC
events to the Kafka topic `ecommerce.orders_db.orders`.

## Relevant Product Docs

- `docs/product/pipeline.md`

## Acceptance Criteria

- Connector `orders-connector` is registered via Debezium REST API.
- `GET /connectors/orders-connector/status` returns `connector.state = RUNNING`.
- After creating an order via the API, a message appears in topic `ecommerce.orders_db.orders`.
- The message envelope matches the CDC event shape in `docs/product/pipeline.md` (`before`, `after`, `op`, `ts_ms`).
- Schema history is stored in Kafka topic `schema-changes.orders`.

## Design Notes

- Commands: `curl -X POST http://localhost:8083/connectors` with connector JSON
- Queries: `curl http://localhost:8083/connectors/orders-connector/status`
- API: Debezium Connect REST API at :8083
- Tables: watches `orders_db.orders` only (not `order_items`)
- Domain rules: connector uses ROW binlog format and GTID — both already configured in Docker Compose MySQL
- UI surfaces: Kafka-UI at :8081 for topic inspection

## Validation

| Layer | Expected proof |
| --- | --- |
| Unit | `CdcEvent_test` — 3 tests confirming JSON deserialization of Debezium envelope |
| Integration | Connector status = RUNNING; Kafka topic receives message after POST /orders |
| E2E | none |
| Platform | Debezium Connect REST API on Docker network |
| Release | none |

## Harness Delta

none

## Evidence

- `CdcEvent_test` → 3 passed (unit proof for envelope deserialization)
- Integration proof pending: requires `docker compose up -d`
