# Architecture

## Solution Structure

```
EventPipeline-Harness/
├── src/
│   ├── EventPipeline.Api         ASP.NET Core 10 — HTTP interface, DI wiring
│   ├── EventPipeline.Data        EF Core 9 — entities, DbContext, migrations
│   ├── EventPipeline.Services    Business logic — OrderService
│   └── EventPipeline.Worker      Background service — Kafka consumer, handlers, clients
└── test/
    └── EventPipeline.Tests       xUnit 2.9 + Moq 4.20
```

## Project Dependencies

```
EventPipeline.Api
  → EventPipeline.Data
  → EventPipeline.Services

EventPipeline.Services
  → EventPipeline.Data

EventPipeline.Worker
  → EventPipeline.Data

EventPipeline.Tests
  → EventPipeline.Data
  → EventPipeline.Services
  → EventPipeline.Worker
```

Inner layers (Data, Services) must not reference outer layers (Api, Worker).

## Tech Stack

| Layer | Technology | Version |
| --- | --- | --- |
| Language | C# / .NET | 10.0 |
| API | ASP.NET Core | 10 |
| ORM | Entity Framework Core + Pomelo MySQL | 9.0 |
| Database | MySQL | 8.0 |
| CDC | Debezium | 3.0.0.Final |
| Messaging | Kafka — Confluent.Kafka | 2.6 |
| Search | Elasticsearch — NEST | 8 / 7.17 |
| Analytics | ClickHouse — ClickHouse.Client | 7.4 |
| Tests | xUnit + Moq | 2.9 / 4.20 |
| Dev infra | Docker Compose | — |

Stack choices are recorded in `docs/decisions/` when they meaningfully constrain
future work.

## Layer Responsibilities

**EventPipeline.Data**
Owns the database schema, EF Core mapping, and migration history. No business
rules. No navigation properties on entities — all related data is loaded
explicitly in the service layer.

**EventPipeline.Services**
Owns business rules: `TotalAmount` computation, status transitions, UTC timestamp
enforcement. Depends only on `EventPipeline.Data`. Returns typed response records,
never entities.

**EventPipeline.Api**
Thin HTTP layer. Controllers delegate entirely to `IOrderService`. No business
logic here. Registers DI in `Program.cs`.

**EventPipeline.Worker**
Kafka consumer pipeline. Receives CDC events from Debezium, extracts the
`payload` from the Debezium envelope, deserializes into `CdcEvent<OrderSnapshot>`,
and fans out to `OrderSearchHandler` and `OrderAnalyticsHandler` in sequence.
Failed messages are routed to DLQ by `KafkaCdcConsumerBase`.

## Boundary Rules

**Parse at the Debezium boundary**
CDC messages arrive as `{"schema":{...},"payload":{...}}`. The consumer extracts
`payload` before deserializing into typed models. Inner handlers never see raw
Kafka message bytes.

**DateTime always UTC**
`CreatedAt` and `UpdatedAt` are set in the service layer as `DateTime.UtcNow`.
A value converter in `AppDbContext` normalizes both directions. Debezium delivers
`DATETIME(6)` columns as microseconds since Unix epoch — `ClickHouseClient`
converts with `DateTime.UnixEpoch.AddMicroseconds(value)`.

**Decimal precision**
`TotalAmount` and `Price` are `decimal(18,2)` in the schema. Debezium connector
uses `decimal.handling.mode=double` to deliver numeric JSON values that
deserialize to `decimal` without precision loss at this scale.

**Offset commit after DLQ**
The consumer always commits the Kafka offset after routing a message to the DLQ.
This prevents infinite retry loops on poison messages. See `docs/decisions/0007`.
