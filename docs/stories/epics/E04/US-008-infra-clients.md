# US-008 Infrastructure Clients — Elasticsearch and ClickHouse

## Status

implemented

## Lane

high-risk

## Product Contract

Two thin infrastructure clients wrap HttpClient (Elasticsearch) and ClickHouse.Client
behind interfaces. Each has a single responsibility: upsert/delete for Elasticsearch,
insert for ClickHouse.

## Relevant Product Docs

- `docs/product/pipeline.md`

## Acceptance Criteria

- `IElasticsearchClient` exposes `UpsertAsync(long id, OrderSnapshot document)` and `DeleteAsync(long id)`.
- `ElasticsearchClient` uses `HttpClient` to PUT/DELETE documents to the configured index.
- `IClickHouseClient` exposes `InsertAsync(OrderSnapshot snapshot, string op, long tsMs, int kafkaPartition, long kafkaOffset)`.
- `ClickHouseClient` creates the `order_events` table on first use if it does not exist, then inserts.
- Both interfaces are mockable via Moq (used in handler tests US-009).

## Design Notes

- Commands: HTTP `PUT /{index}/_doc/{id}`, `DELETE /{index}/_doc/{id}`; ClickHouse INSERT
- Queries: none
- API: Elasticsearch at :9200; ClickHouse at :8123
- Tables: `order_events` (ClickHouse) — created by client if absent
- Domain rules: timestamp fields in OrderSnapshot are microseconds since Unix epoch; client converts to DateTime
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

- `dotnet build src/EventPipeline.Worker` → succeeded
- Unit proof: interfaces mocked via Moq in `OrderSearchHandler_test` (4) + `OrderAnalyticsHandler_test` (3) — all passed
- Integration proof: ClickHouse insert and Elasticsearch upsert/delete verified in US-010 E2E
