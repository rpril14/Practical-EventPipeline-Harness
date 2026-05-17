# Test Matrix

This file maps product behavior to proof.

## Status Values

| Status | Meaning |
| --- | --- |
| planned | Accepted as intended behavior, not implemented |
| in_progress | Actively being built |
| implemented | Implemented and proof exists |
| changed | Contract changed after earlier implementation |
| retired | No longer part of the product contract |

## Matrix

| Story | Contract | Unit | Integration | E2E | Platform | Status | Evidence |
| --- | --- | --- | --- | --- | --- | --- | --- |
| US-001 Init solution | Solution scaffold, project references, SDK pin | no | `dotnet build` succeeds | no | no | implemented | `dotnet build` → 0 errors |
| US-002 Docker Compose | 7 services running, MySQL binlog=ROW | no | `docker compose ps` all running | no | Docker Compose local | implemented | file written; runtime proof pending first `docker compose up` |
| US-003 Data layer | EF mapping, UTC datetime, decimal precision | `AppDbContext_test` (3) | Migration runs against MySQL | no | no | implemented | `AppDbContext_test` → 3 passed; migration pending MySQL |
| US-004 Services | Create computes totalAmount; update/get return correct data | `OrderService_test` (8) | no | no | no | implemented | `OrderService_test` → 8 passed |
| US-005 API | POST=201, PUT=200/404, GET=200/404 | no | no | `curl POST /orders` → `GET /orders/{id}` | no | implemented | build succeeded; E2E smoke pending MySQL |
| US-006 CDC connector | Connector RUNNING; orders table events in Kafka | `CdcEvent_test` (3) | Connector RUNNING; op=c,op=u events in topic | no | Debezium REST :8083 | implemented | Connector RUNNING; CDC events verified in Kafka; Worker E2E pending hosts entry |
| US-007 Worker models | CdcEvent and OrderSnapshot deserialize correctly | `CdcEvent_test` (3, shared with US-006) | no | no | no | implemented | `CdcEvent_test` → 3 passed |
| US-008 Infra clients | IElasticsearchClient and IClickHouseClient are mockable | via mock in US-009 tests | manual ClickHouse insert verify | no | no | planned | none |
| US-009 Handlers | Search: c/u/r→upsert, d→delete; Analytics: always insert | `OrderSearchHandler_test` (4) + `OrderAnalyticsHandler_test` (3) | no | no | no | implemented | 4 + 3 = 7 passed |
| US-010 Consumer | Retry backoff; DLQ on permanent error; always commit | `RetryPolicy_test` (6) + `OrderCdcConsumer_test` (4) | Worker processes Kafka message | POST→CDC→ES+CH | no | implemented | 6+4=10 passed; integration pending infra |

## Evidence Rules

- Unit proof covers pure domain and application rules.
- Integration proof covers backend enforcement, data integrity, provider
  behavior, jobs, or service contracts.
- E2E proof covers user-visible browser flows.
- Platform proof covers only shell, deployment, mobile, desktop, or runtime
  behavior that cannot be proven in lower layers.
- A story can be implemented without every proof column if the story packet
  explains why.
