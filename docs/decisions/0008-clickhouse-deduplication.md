# 0008 ClickHouse Deduplication via ReplacingMergeTree

Date: 2026-05-19

## Status

Accepted

## Context

Kafka guarantees at-least-once delivery. If the Worker crashes after inserting a row
into ClickHouse but before committing the Kafka offset, the same message is redelivered
on restart. ClickHouse's append-only `order_events` table would then contain a
duplicate row. For analytics queries this causes double-counting.

## Decision

Change `order_events` to use `ReplacingMergeTree` with `ORDER BY (KafkaPartition,
KafkaOffset)`. A Kafka partition+offset pair uniquely identifies a message; a replayed
message produces an identical pair, which ClickHouse deduplicates at merge time.

Two columns are added: `KafkaPartition Int32` and `KafkaOffset Int64`. These flow from
`ConsumeResult` in `KafkaCdcConsumerBase` through `KafkaMessageContext` to
`OrderAnalyticsHandler` and into `IClickHouseClient.InsertAsync`.

Queries against `order_events` should use `FINAL` to force deduplication before the
background merge completes.

## Alternatives Considered

1. **MergeTree + check-before-insert** — SELECT before INSERT is not atomic in
   ClickHouse and is expensive at insert throughput. Rejected.
2. **Generated event ID string** — Concatenating partition and offset into a string
   carries the same information at higher cost (string comparison, extra code).
   Rejected in favour of the natural integer pair.

## Consequences

Positive:

- Replayed messages produce no observable duplicates in analytics queries that use `FINAL`.
- `KafkaPartition` and `KafkaOffset` columns are useful for debugging: every row can be
  traced back to its source message.

Tradeoffs:

- Deduplication is eventual, not immediate. Between insert and background merge, a
  duplicate row may appear without `FINAL`. `FINAL` adds query overhead.
- Existing `order_events` tables created under the previous `MergeTree` schema are
  incompatible. Drop and recreate the table when upgrading.
- `ORDER BY (KafkaPartition, KafkaOffset)` means range queries by `Id` or `TsMs` are
  not index-optimised. Add a projection if those query patterns become common.

## Follow-Up

- Document `FINAL` requirement for analytics queries in `docs/product/pipeline.md`.
- Drop the existing `order_events` table before the first run after this change.
