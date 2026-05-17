# US-007 Worker Models and Options

## Status

implemented

## Lane

normal

## Product Contract

The worker layer has typed models for the Debezium CDC envelope and configuration
option classes for Kafka, Elasticsearch, and ClickHouse.

## Relevant Product Docs

- `docs/product/pipeline.md`

## Acceptance Criteria

- `CdcEvent<T>` record deserializes `before`, `after`, `op`, `ts_ms` from Debezium JSON (snake_case).
- `OrderSnapshot` record maps MySQL fields from Debezium (snake_case field names).
- `KafkaOptions`, `ElasticsearchOptions`, `ClickHouseOptions` option classes exist with correct properties.
- `CdcEvent_test`: 3 tests pass — create op, delete op, update op deserialization.

## Design Notes

- Commands: none
- Queries: none
- API: none
- Tables: none
- Domain rules: `CdcEvent<T>` is generic; `OrderSnapshot` maps to MySQL snake_case column names
- UI surfaces: none

## Validation

| Layer | Expected proof |
| --- | --- |
| Unit | `CdcEvent_test` — 3 tests covering c/d/u op deserialization |
| Integration | none |
| E2E | none |
| Platform | none |
| Release | none |

## Harness Delta

none

## Evidence

- `CdcEvent_test` → 3 passed (c/d/u op deserialization)
