# Product Overview

## What This System Does

EventPipeline is an event-driven order processing backend. It accepts REST commands
to create and update orders, persists transactional state in MySQL, and propagates
change events in real time to a search index and an analytics store.

## Who Uses It

Backend engineers and data consumers. There are no end-user-facing surfaces in v1.

## Data Flow

```
Client
  │
  ▼
REST API  ──────────────────────────────────────────────────────────────────────►  MySQL
                                                                                      │
                                                                                      │ binlog (ROW, GTID)
                                                                                      ▼
                                                                               Debezium Connect
                                                                                      │
                                                                                      ▼
                                                                         Kafka: ecommerce.orders_db.orders
                                                                                      │
                                                            ┌─────────────────────────┤
                                                            ▼                         ▼
                                                  OrderSearchHandler        OrderAnalyticsHandler
                                                            │                         │
                                                            ▼                         ▼
                                                    Elasticsearch               ClickHouse
                                                   (current state)            (event append log)

On error → ecommerce.orders_db.orders.dlq (after 5 retry attempts)
```

## Tech Stack

| Layer | Technology |
| --- | --- |
| Language | C# / .NET 10 |
| API | ASP.NET Core 10 |
| ORM | Entity Framework Core 9 + Pomelo MySQL |
| Database | MySQL 8.0 |
| CDC | Debezium 3.0 |
| Messaging | Kafka (Confluent.Kafka 2.6) |
| Search | Elasticsearch 8 (NEST 7.17) |
| Analytics | ClickHouse (ClickHouse.Client 7.4) |
| Tests | xUnit 2.9 + Moq 4.20 |
| Dev infra | Docker Compose |

## Out of Scope (v1)

- Authentication and authorization
- Multi-tenancy
- Production deployment (CI/CD, cloud infra)
- Order items CDC (Debezium watches `orders` table only)
