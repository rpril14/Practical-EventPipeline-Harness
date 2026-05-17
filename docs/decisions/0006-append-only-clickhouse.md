# 0006 Append-Only ClickHouse Analytics Store

Date: 2026-05-17

## Status

Accepted

## Context

ClickHouse is used as the analytics store for order events. The question was
whether to maintain current state (upsert) or record every event as an immutable
row (append-only). Elasticsearch already maintains current state via upsert/delete.

## Decision

ClickHouse is append-only. Every CDC event (`c`, `u`, `d`, `r`) produces one new
row in `order_events`. The `Op` and `TsMs` columns allow consumers to reconstruct
the full change history or aggregate by time window.

## Alternatives Considered

1. Upsert in ClickHouse using ReplacingMergeTree — keeps current state in
   ClickHouse too, but loses event history and duplicates the Elasticsearch
   responsibility.
2. Only write to ClickHouse on specific ops (e.g. `c` and `u`) — loses delete
   events from the audit trail.

## Consequences

Positive:

- Full event history is available for time-series analytics and auditing.
- Append-only is idiomatic for ClickHouse; inserts are fast.
- Elasticsearch and ClickHouse serve clearly distinct responsibilities.

Tradeoffs:

- ClickHouse does not reflect current order state directly; queries that need
  "latest status" must use a `argMax` aggregation or a separate view.

## Follow-Up

- If a "current state" view is needed in ClickHouse, add a materialised view
  on top of `order_events` rather than changing the write model.
