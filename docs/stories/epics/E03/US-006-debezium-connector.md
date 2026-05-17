# US-006 Debezium Connector Registration

## Status

implemented

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

- `CdcEvent_test` → 3 passed (envelope deserialization, PascalCase field names)
- Connector `orders-connector` registered via Debezium REST API
- `GET /connectors/orders-connector/status` → `connector.state=RUNNING, task.state=RUNNING`
- Created order via `POST /orders` → CDC event appears in topic `ecommerce.orders_db.Orders`
  - `op=c id=1 status=1 total=60.0`
  - `op=u id=1 status=2 total=60.0` (after status update)
- Kafka config: `confluentinc/cp-kafka:7.9.0`, KRaft mode, `decimal.handling.mode=double`
- Note: Worker E2E requires `127.0.0.1 kafka` in hosts file (one-time admin setup)
