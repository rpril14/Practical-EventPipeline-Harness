# CDC Pipeline

## Overview

Debezium reads the MySQL binlog and publishes CDC events to Kafka. The worker
consumes those events and fans out to two downstream stores.

## CDC Event Shape (Debezium Envelope)

```json
{
  "before": { "id": 1, "customer_id": 2, "status": 1, "total_amount": 50.00, "created_at": 0, "updated_at": 0 },
  "after":  { "id": 1, "customer_id": 2, "status": 2, "total_amount": 50.00, "created_at": 0, "updated_at": 0 },
  "op": "u",
  "ts_ms": 1700000000000
}
```

`op` values:
- `c` — row created
- `u` — row updated
- `d` — row deleted
- `r` — snapshot read (initial sync)

`created_at` and `updated_at` are delivered as milliseconds since Unix epoch.

Kafka topic: `ecommerce.orders_db.orders`
DLQ topic: `ecommerce.orders_db.orders.dlq`

## Elasticsearch Behavior

| op | Action |
| --- | --- |
| c, u, r | Upsert document using `after` |
| d | Delete document using `before.id` |

Maintains current order state. Index name: `orders`.

## ClickHouse Behavior

Every event is appended regardless of `op`. Uses `after ?? before` as the snapshot.

Table: `order_events`

```sql
CREATE TABLE IF NOT EXISTS order_events (
  Id           UInt64,
  CustomerId   UInt64,
  Status       Int32,
  TotalAmount  Decimal64(2),
  CreatedAt    DateTime,
  UpdatedAt    DateTime,
  Op           String,
  TsMs         Int64
) ENGINE = MergeTree() ORDER BY (Id, TsMs)
```

Table is created on worker startup if it does not exist. Never updated or deleted.

## Retry Policy

- Max attempts: 5
- Backoff: `2^(attempt-1)` seconds → 1s, 2s, 4s, 8s, 16s
- Retry on: `HttpRequestException`, `SocketException`, `TimeoutException`, `IOException`
- Immediate DLQ on: all other exceptions (e.g. `JsonException`, `ArgumentException`)

## DLQ Behavior

On permanent failure or retry exhaustion:
1. Publish raw message value to DLQ topic.
2. Commit offset regardless — prevents infinite retry loop on poison messages.
