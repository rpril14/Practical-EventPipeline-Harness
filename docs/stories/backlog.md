# Story Backlog

## Epics

| Epic | Description | Status |
| --- | --- | --- |
| E01 | Infrastructure & Data Layer — solution, Docker Compose, EF Core | sliced |
| E02 | API Layer — OrdersController, OrderService, DI | sliced |
| E03 | CDC Pipeline — Debezium connector, Kafka topic | sliced |
| E04 | Worker Layer — consumer, handlers, RetryPolicy, DLQ | sliced |

## In Progress

none

## Planned

| Story | Epic | Lane | Description |
| --- | --- | --- | --- |
| [US-001](epics/E01/US-001-init-solution.md) | E01 | normal | Initialize .NET 10 solution and project structure |
| [US-002](epics/E01/US-002-docker-compose.md) | E01 | normal | Docker Compose — 7 infrastructure services |
| [US-003](epics/E01/US-003-data-layer.md) | E01 | normal | Data layer — OrderEntity, AppDbContext, EF migration |
| [US-004](epics/E02/US-004-services-layer.md) | E02 | normal | Services layer — OrderService |
| [US-005](epics/E02/US-005-api-layer.md) | E02 | normal | API layer — OrdersController, Program.cs |
| [US-006](epics/E03/US-006-debezium-connector.md) | E03 | high-risk | Debezium connector and CDC event verification |
| [US-007](epics/E04/US-007-worker-models.md) | E04 | normal | Worker models — CdcEvent, OrderSnapshot, options |
| [US-008](epics/E04/US-008-infra-clients.md) | E04 | high-risk | Infrastructure clients — Elasticsearch, ClickHouse |
| [US-009](epics/E04/US-009-handlers.md) | E04 | high-risk | Event handlers — search and analytics |
| [US-010](epics/E04/US-010-consumer.md) | E04 | high-risk | Kafka consumer — base, OrderCdcConsumer, RetryPolicy, DLQ |

## Completed

none

## Deferred

none
