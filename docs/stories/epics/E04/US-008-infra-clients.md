# US-008 Infrastructure Clients — Elasticsearch and ClickHouse

## Status

planned

## Lane

high-risk

## Product Contract

Two thin infrastructure clients wrap NEST and ClickHouse.Client behind interfaces.
Each has a single responsibility: upsert/delete for Elasticsearch, insert for
ClickHouse.

## Relevant Product Docs

- `docs/product/pipeline.md`

## Acceptance Criteria

- `IElasticsearchClient` exposes `UpsertAsync(long id, OrderSnapshot document)` and `DeleteAsync(long id)`.
- `ElasticsearchClient` wraps NEST and indexes to the configured index.
- `IClickHouseClient` exposes `InsertAsync(OrderSnapshot snapshot, string op, long tsMs)`.
- `ClickHouseClient` creates the `order_events` table on first use if it does not exist, then inserts.
- Both interfaces are mockable via Moq (used in handler tests US-009).

## Design Notes

- Commands: NEST `IndexAsync`, `DeleteAsync`; ClickHouse INSERT
- Queries: none
- API: Elasticsearch at :9200; ClickHouse at :8123
- Tables: `order_events` (ClickHouse) — created by client if absent
- Domain rules: timestamp fields in OrderSnapshot are milliseconds since epoch; client converts to DateTime
- UI surfaces: none

## Validation

| Layer | Expected proof |
| --- | --- |
| Unit | tested via mocks in `OrderSearchHandler_test` and `OrderAnalyticsHandler_test` |
| Integration | manual verification: insert a row into ClickHouse, query via HTTP API |
| E2E | covered by US-010 end-to-end |
| Platform | none |
| Release | none |

## Harness Delta

none

## Evidence
