# US-009 Event Handlers — OrderSearchHandler and OrderAnalyticsHandler

## Status

implemented

## Lane

high-risk

## Product Contract

Two handlers implement `IOrderEventHandler`. OrderSearchHandler maintains current
order state in Elasticsearch. OrderAnalyticsHandler appends every event to
ClickHouse.

## Relevant Product Docs

- `docs/product/pipeline.md`

## Acceptance Criteria

- `IOrderEventHandler` exposes `HandleAsync(CdcEvent<OrderSnapshot>)`.
- `OrderSearchHandler`: op=d → DeleteAsync; op=c/u/r → UpsertAsync with `After`.
- `OrderAnalyticsHandler`: always calls InsertAsync using `After ?? Before`.
- `OrderSearchHandler_test`: 4 tests pass (c, u, r → upsert; d → delete).
- `OrderAnalyticsHandler_test`: 3 tests pass (c → After; u → After; d → Before).

## Design Notes

- Commands: IElasticsearchClient.UpsertAsync, DeleteAsync; IClickHouseClient.InsertAsync
- Queries: none
- API: none
- Tables: none
- Domain rules: handler logic is purely op-routing; no business rules
- UI surfaces: none

## Validation

| Layer | Expected proof |
| --- | --- |
| Unit | `OrderSearchHandler_test` (4 tests) + `OrderAnalyticsHandler_test` (3 tests) |
| Integration | none |
| E2E | covered by US-010 |
| Platform | none |
| Release | none |

## Harness Delta

none

## Evidence

- `OrderSearchHandler_test` → 4 passed (c, u, r → upsert; d → delete)
- `OrderAnalyticsHandler_test` → 3 passed (c/u → After; d → Before)
