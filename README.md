# EventPipeline — Practical Harness Demo

An event-driven e-commerce order processing system, built end-to-end using the
**Harness Engineering** methodology: every feature started as a product contract,
moved through a story packet, and was proven before being marked done.

This repository is a working demonstration that AI-assisted development can be
reliable and inspectable — not just fast.

## What This System Does

A REST API accepts commands to create and update orders. MySQL stores transactional
state. Debezium reads the MySQL binlog and publishes CDC events to Kafka. A
background worker consumes those events and fans out to Elasticsearch (search index)
and ClickHouse (analytics store).

```
Client → API → MySQL → Debezium → Kafka → Worker → Elasticsearch
                                                  └──────────────→ ClickHouse
```

## Tech Stack

| Layer | Technology |
|---|---|
| Language | C# / .NET 10 |
| API | ASP.NET Core 10 |
| ORM | Entity Framework Core 9 + Pomelo MySQL |
| Database | MySQL 8.0 |
| CDC | Debezium 3.0 |
| Messaging | Kafka (Confluent.Kafka 2.6) |
| Search | Elasticsearch 8 (HttpClient) |
| Analytics | ClickHouse (ClickHouse.Client 7.4) |
| Tests | xUnit 2.9 + Moq 4.20 |
| CI | GitHub Actions |
| Dev infra | Docker Compose |

## How It Was Built — The Harness Flow

This project does not start from code. Every change passed through a structured
intake process before a line was written:

```
Spec provided by human
  → Intake classification (new spec, high-risk, 4+ risk flags)
  → Product contracts written (docs/product/)
  → Story packets created (docs/stories/epics/)
  → Test matrix populated (docs/TEST_MATRIX.md)
  → Architecture decisions recorded (docs/decisions/)
  → Implementation: failing test → implement → pass → commit
  → Story status updated to implemented
  → Test matrix evidence filled in
```

Each of the 10 stories in this project followed that loop. The harness artifacts
are not documentation added after the fact — they drove the work.

## Project Structure

```
EventPipeline-Harness/
├── AGENTS.md                        ← agent entrypoint: read order, task loop, done definition
├── src/
│   ├── EventPipeline.Api            ← ASP.NET Core: POST/PUT/GET /orders
│   ├── EventPipeline.Data           ← EF Core entities, DbContext, migrations
│   ├── EventPipeline.Services       ← OrderService business logic
│   └── EventPipeline.Worker         ← Kafka consumer, scoped handlers, DLQ routing
├── test/
│   └── EventPipeline.Tests          ← 25 tests: unit + integration coverage
├── scripts/
│   └── docker-compose.yml           ← MySQL, Kafka, Debezium, Elasticsearch, ClickHouse
└── docs/
    ├── product/                     ← living product contracts (orders, pipeline)
    ├── stories/
    │   ├── backlog.md               ← 10 stories across 4 epics
    │   ├── spec-intake-2026-05-17.md
    │   └── epics/E01…E04/           ← individual story packets
    ├── decisions/                   ← 8 architecture decisions with context and tradeoffs
    ├── TEST_MATRIX.md               ← behavior-to-proof control panel
    └── templates/                   ← reusable story, decision, validation templates
```

## Running Locally

**Prerequisites:** Docker Desktop, .NET 10 SDK, `127.0.0.1 kafka` in hosts file.

```bash
# Start infrastructure
cd scripts && docker compose up -d

# Apply database migration
dotnet ef database update \
  --project src/EventPipeline.Data \
  --startup-project src/EventPipeline.Api

# Register Debezium connector
curl -X POST http://localhost:8083/connectors \
  -H "Content-Type: application/json" \
  -d '{
    "name": "orders-connector",
    "config": {
      "connector.class": "io.debezium.connector.mysql.MySqlConnector",
      "database.hostname": "mysql", "database.port": "3306",
      "database.user": "root", "database.password": "root",
      "database.server.id": "1", "topic.prefix": "ecommerce",
      "database.include.list": "orders_db",
      "table.include.list": "orders_db.Orders",
      "schema.history.internal.kafka.bootstrap.servers": "kafka:9092",
      "schema.history.internal.kafka.topic": "schema-changes.orders",
      "decimal.handling.mode": "double"
    }
  }'

# Start API
dotnet run --project src/EventPipeline.Api --urls http://localhost:5000

# Start Worker (separate terminal)
dotnet run --project src/EventPipeline.Worker

# Create an order
curl -X POST http://localhost:5000/orders \
  -H "Content-Type: application/json" \
  -d '{"customerId":1,"items":[{"productId":10,"quantity":2,"price":15.00}]}'
```

After creating an order, the CDC event flows through Debezium → Kafka → Worker and
the order appears in both Elasticsearch (`http://localhost:9200/orders/_search`) and
ClickHouse (`SELECT * FROM order_events`). ClickHouse uses a ReplacingMergeTree table
so re-delivered events are deduplicated by `(order_id, kafka_offset)`.

## Running Tests

```bash
dotnet test
# Expected: 25 passed, 0 failed
```

## What the Harness Provides

**`AGENTS.md`** tells any AI agent what to read first, how to classify work, when to
ask for confirmation, and what "done" means. An agent entering this repo cold can
orient itself without relying on chat history.

**`docs/product/`** contains the living product contracts derived from the original
spec. These are updated as behavior changes — not kept as a static spec document.

**`docs/stories/epics/`** holds one story packet per feature. Each packet records the
product contract, acceptance criteria, design notes, validation shape, and evidence
gathered after implementation. The story is not closed until proof exists.

**`docs/TEST_MATRIX.md`** maps every behavior to its proof. A row is marked
`implemented` only when tests or manual evidence have been recorded.

**`docs/decisions/`** captures eight architecture decisions — why the status enum is
closed, why navigation properties are absent, why ClickHouse is append-only, why the
consumer always commits after DLQ routing, and why ClickHouse deduplication uses
ReplacingMergeTree. Future developers and agents inherit the reasoning, not just the
outcome.

## What Harness Engineering Is

Coding agents are capable enough to participate in real software work. But the model
alone is not the whole system. A repository also needs clear instructions, shared
product truth, validation loops, and decision records so an agent can understand what
matters before it changes code.

Harness Engineering is the practice of designing that operating environment. The goal
is not to make AI write code faster. The goal is to make AI-assisted development more
reliable, inspectable, and easier for humans to steer.

This project is a concrete example of that practice applied to a non-trivial system.
