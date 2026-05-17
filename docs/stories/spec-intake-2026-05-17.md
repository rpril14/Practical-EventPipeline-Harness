# Spec Intake

Date: 2026-05-17

## Source

- User prompt: EventPipeline — event-driven e-commerce order processing system spec provided directly in chat.

## Project Summary

An internal order processing backend for an e-commerce platform. The system accepts
REST commands to create and update orders, persists state in MySQL, and propagates
changes in real time to a search index (Elasticsearch) and an analytics store
(ClickHouse) via a CDC pipeline powered by Debezium and Kafka.

Users of the system are backend engineers and data consumers — not end users.

## Candidate Product Docs

| File | Purpose | Source sections |
| --- | --- | --- |
| `docs/product/overview.md` | System purpose, data flow, out of scope | Spec overview + data flow diagram |
| `docs/product/orders.md` | Order entity contract, status enum, CRUD rules | Spec steps 4–6 |
| `docs/product/pipeline.md` | CDC event contract, worker behavior, retry, DLQ | Spec steps 7, 9 |

## Candidate Epics

| Epic | Description | Status |
| --- | --- | --- |
| E01 | Infrastructure & Data Layer — solution scaffold, Docker Compose, EF Core | sliced |
| E02 | API Layer — OrdersController, OrderService, DI wiring | sliced |
| E03 | CDC Pipeline — Debezium connector, Kafka topic verification | sliced |
| E04 | Worker Layer — consumer, handlers, RetryPolicy, DLQ | sliced |

## Architecture Questions

- Runtime stack: .NET 10, ASP.NET Core 10, EF Core 9 + Pomelo MySQL
- Product surfaces: REST API (internal), background worker
- Storage: MySQL 8.0 (transactional), Elasticsearch 8 (search), ClickHouse (analytics)
- External providers: Kafka (Confluent.Kafka 2.6), Debezium 3.0, NEST 7.17, ClickHouse.Client 7.4
- Deployment target: Docker Compose for dev; production target not defined in spec
- Security model: not defined in spec — no auth, no authorization layer in v1

## Validation Shape

| Layer | Expected proof |
| --- | --- |
| Unit | EF mapping rules, OrderService logic, handler routing per op, RetryPolicy backoff |
| Integration | Migrations run clean against MySQL, CDC events published to Kafka |
| E2E | POST order → GET order; CDC event flows through to Elasticsearch and ClickHouse |
| Platform | POSIX shell for Docker Compose commands; `dotnet test` on local dev machine |
| Release | Not defined — no CI pipeline in spec |

## Open Decisions

- Status enum values closed at 5 — adding new statuses is a breaking data model change.
- ClickHouse table created on worker startup — no migration tooling defined.
- No authentication or multi-tenancy in scope.
- Debezium password stored in plain text in connector registration curl — acceptable for dev, not for prod.

## First Story Candidates

- US-001: Initialize .NET solution and project structure
- US-002: Docker Compose — all seven infrastructure services
- US-003: Data layer — OrderEntity, AppDbContext, EF migration
- US-004: Services layer — OrderService (create, updateStatus, get)
- US-005: API layer — OrdersController, Program.cs DI
- US-006: Debezium connector registration and CDC event verification
- US-007: Worker models — CdcEvent, OrderSnapshot, WorkerOptions
- US-008: Infrastructure clients — ElasticsearchClient, ClickHouseClient
- US-009: Event handlers — OrderSearchHandler, OrderAnalyticsHandler
- US-010: Kafka consumer — KafkaCdcConsumerBase, OrderCdcConsumer, RetryPolicy, DLQ

## Harness Delta

- `docs/product/` populated for the first time.
- `docs/stories/epics/` structure created from spec epics.
- `docs/TEST_MATRIX.md` populated with candidate rows.
- `docs/decisions/` extended with four project-specific decisions (0004–0007).
