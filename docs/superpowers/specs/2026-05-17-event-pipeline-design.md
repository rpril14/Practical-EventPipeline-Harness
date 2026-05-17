# EventPipeline — Design Doc

Date: 2026-05-17
Status: awaiting implementation plan

---

## What We Are Building

An event-driven e-commerce order processing system. A REST API accepts commands to
create and update orders. MySQL stores transactional state. Debezium reads the MySQL
binlog and publishes CDC events to Kafka. A background worker consumes those events
and fans out to Elasticsearch (search index) and ClickHouse (analytics store).

---

## Intake Classification

```
Type:   new spec
Lane:   high-risk

Risk flags:
  ✓ Data model      — MySQL schema + EF Core migrations
  ✓ External systems — Kafka, Debezium, Elasticsearch, ClickHouse
  ✓ Public contracts — REST API shape (POST/PUT/GET /orders)
  ✓ Multi-domain    — orders, search, analytics change together
```

---

## Tech Stack

| Layer       | Technology                              |
|-------------|-----------------------------------------|
| Language    | C# / .NET 10                            |
| API         | ASP.NET Core 10                         |
| ORM         | Entity Framework Core 9 + Pomelo MySQL  |
| Database    | MySQL 8.0                               |
| CDC         | Debezium 3.0                            |
| Messaging   | Kafka (Confluent.Kafka 2.6)             |
| Search      | Elasticsearch 8 (NEST 7.17)            |
| Analytics   | ClickHouse (ClickHouse.Client 7.4)      |
| Tests       | xUnit 2.9 + Moq 4.20                   |
| Dev infra   | Docker Compose                          |

---

## Architecture

```
Client
  │
  ▼
OrdersController (POST /orders, PUT /orders/{id}/status, GET /orders/{id})
  │
  ▼
OrderService
  │
  ▼
AppDbContext → MySQL 8.0
  │
  │ binlog (ROW format, GTID ON)
  ▼
Debezium Connect
  │
  ▼
Kafka topic: ecommerce.orders_db.orders
  │
  ├──────────────────────────┐
  ▼                          ▼
OrderSearchHandler     OrderAnalyticsHandler
  │                          │
  ▼                          ▼
Elasticsearch          ClickHouse
(upsert / delete)      (append-only insert)

On error:
  └─> ecommerce.orders_db.orders.dlq (after 5 retry attempts)
```

---

## Solution Structure

```
EventPipeline-harness/
├── src/
│   ├── EventPipeline.Api         — ASP.NET Core, controllers, DI
│   ├── EventPipeline.Data        — EF Core, entities, DbContext, migrations
│   ├── EventPipeline.Services    — business logic, OrderService
│   └── EventPipeline.Worker      — Kafka consumer, handlers, retry, DLQ
└── test/
    └── EventPipeline.Tests       — xUnit tests for all layers
```

---

## Epics

### E01 — Infrastructure & Data Layer

Solution scaffold, Docker Compose (MySQL, Kafka, Debezium, Elasticsearch,
ClickHouse, Adminer, Kafka-UI), EF Core entities, DbContext, migration.

Lane: normal

Candidate stories:
- US-001: Initialize solution and projects
- US-002: Docker Compose infrastructure
- US-003: Data layer — entities, DbContext, migration

### E02 — API Layer

OrdersController, OrderService, DI registration.

Lane: normal

Candidate stories:
- US-004: Services layer — OrderService (create, updateStatus, get)
- US-005: API layer — OrdersController, Program.cs

### E03 — CDC Pipeline

Debezium connector registration, Kafka topic verification.

Lane: high-risk (external systems, schema contract)

Candidate stories:
- US-006: Register Debezium connector, verify CDC event shape

### E04 — Worker Layer

KafkaCdcConsumerBase, OrderCdcConsumer, handlers, RetryPolicy, DLQ.

Lane: high-risk (external systems, multi-domain)

Candidate stories:
- US-007: Worker models — CdcEvent, OrderSnapshot, WorkerOptions
- US-008: Infrastructure clients — ElasticsearchClient, ClickHouseClient
- US-009: Handlers — OrderSearchHandler, OrderAnalyticsHandler
- US-010: Consumer — KafkaCdcConsumerBase, OrderCdcConsumer, RetryPolicy, DLQ

---

## Product Contracts

### Orders API

`POST /orders`
- Body: `{ customerId, items: [{ productId, quantity, price }] }`
- Computes `totalAmount = sum(quantity * price)`
- Returns 201 with created order

`PUT /orders/{id}/status`
- Body: `{ status }` — one of: Pending=1, Processing, Shipped, Delivered, Cancelled
- Returns 200 with updated order, 404 if not found

`GET /orders/{id}`
- Returns 200 with order + items, 404 if not found

### Order Entity

Fields: `Id, CustomerId, Status, TotalAmount, CreatedAt, UpdatedAt`
- Status is an explicit enum: Pending=1, Processing, Shipped, Delivered, Cancelled
- `CreatedAt` / `UpdatedAt` set manually in service layer, always UTC
- No navigation properties on entities

### OrderItem Entity

Fields: `Id, OrderId, ProductId, Quantity, Price`

### ClickHouse Schema

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

Created on worker startup. Append-only — never updated or deleted.

### CDC Event Shape (Debezium envelope)

```json
{ "before": {...}, "after": {...}, "op": "c|u|d|r", "ts_ms": 1234567890 }
```

`op` values: `c`=create, `u`=update, `d`=delete, `r`=snapshot read

### Worker Behavior

- `op=c/u/r` → Elasticsearch upsert using `After`
- `op=d`     → Elasticsearch delete using `Before`
- All ops    → ClickHouse insert (append-only, uses `After ?? Before`)
- Retry: max 5 attempts, exponential backoff (1s → 2s → 4s → 8s → 16s)
- Retry on: `HttpRequestException`, `SocketException`, `TimeoutException`, `IOException`
- DLQ immediately on: all other exceptions (e.g. `JsonException`, `ArgumentException`)
- Consumer always commits offset after DLQ routing to prevent infinite loops

---

## Validation Shape

| Story | Unit | Integration | E2E |
|-------|------|-------------|-----|
| US-003 Data layer | EF mapping, UTC conversion, decimal precision | Migrations run clean against MySQL | — |
| US-004 Services | OrderService create/update/get logic | Service → real DbContext round-trip | — |
| US-005 API | Controller routes and response codes | Controller → DB | POST → GET order |
| US-006 CDC | Debezium envelope deserialization | Connector publishes events to Kafka | — |
| US-009 Handlers | Handler routing per op | — | — |
| US-010 Consumer | RetryPolicy backoff + DLQ routing | Consumer processes real Kafka message | CDC → ES + CH |

---

## Key Decisions

1. **Explicit status enum** — status drives filtering and future overdue logic;
   free-form labels would require validation at every boundary.

2. **No navigation properties** — keeps DbContext simple, avoids accidental
   N+1 queries across service boundaries.

3. **Append-only ClickHouse** — analytics records every op as an event row;
   Elasticsearch maintains current state via upsert/delete.

4. **Commit after DLQ** — consumer commits offset after routing to DLQ to
   prevent infinite retry loops on poison messages.

5. **Project suffix `-harness`** — distinguishes from an existing project with
   the same base name on this machine.
