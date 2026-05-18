# US-010 Kafka Consumer — KafkaCdcConsumerBase, OrderCdcConsumer, DLQ

## Status

implemented

## Lane

high-risk

## Product Contract

The worker consumes CDC events from Kafka, fans out to both handlers in sequence,
and routes failed messages to the DLQ. Offset is always committed regardless of outcome.

## Relevant Product Docs

- `docs/product/pipeline.md`

## Acceptance Criteria

- `KafkaCdcConsumerBase<T>` is an abstract `IHostedService` that handles the polling loop, DLQ routing, and offset commit. It creates a DI scope per message and passes `IServiceProvider` to `HandleAsync` so each message gets fresh handler instances.
- `OrderCdcConsumer` extends the base and calls all registered `IOrderEventHandler` instances in sequence.
- On success: consumer commits offset.
- On any handler exception: message published to DLQ topic, then offset committed.
- `OrderCdcConsumer_test`: 4 tests pass (both handlers called, DLQ on throw, commit on throw, commit on success).

## Design Notes

- Commands: `consumer.Consume`, `consumer.Commit`, `dlqProducer.ProduceAsync`
- Queries: none
- API: none
- Tables: none
- Domain rules: commit always happens last — even after DLQ route — to prevent infinite loop on poison messages
- UI surfaces: none

## Validation

| Layer | Expected proof |
| --- | --- |
| Unit | `OrderCdcConsumer_test` (4 tests) |
| Integration | Worker starts and processes a Kafka message end-to-end |
| E2E | POST /orders → CDC event → Elasticsearch document exists + ClickHouse row exists |
| Platform | none |
| Release | none |

## Harness Delta

none

## Evidence

- `OrderCdcConsumer_test` → 4 passed (both handlers called, DLQ on throw, commit on throw, commit on success)
- Full suite → 25 passed, 0 failed
- E2E: `POST /orders` (id=4) → CDC event in Kafka → Worker processed → ES count=3, ClickHouse row `Id=4 Op=c TotalAmount=99`
- Fixes applied: Debezium schema wrapper extraction, microsecond timestamp, ClickHouse password
- Change: RetryPolicy removed — DLQ-only error handling per product decision
