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
                                                                         Kafka: ecommerce.orders_db.Orders
                                                                                      │
                                                            ┌─────────────────────────┤
                                                            ▼                         ▼
                                                  OrderSearchHandler        OrderAnalyticsHandler
                                                            │                         │
                                                            ▼                         ▼
                                                    Elasticsearch               ClickHouse
                                                   (current state)            (event append log)

On error → ecommerce.orders_db.Orders.dlq (after 5 retry attempts)
```

## Out of Scope (v1)

- Authentication and authorization
- Multi-tenancy
- Production deployment (CI/CD, cloud infra)
- Order items CDC (Debezium watches `Orders` table only)
