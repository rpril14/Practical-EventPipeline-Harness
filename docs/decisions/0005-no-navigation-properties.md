# 0005 No Navigation Properties on Entities

Date: 2026-05-17

## Status

Accepted

## Context

EF Core supports navigation properties that allow lazy or eager loading of related
entities. In a service-layered architecture where queries are explicit, navigation
properties can cause accidental N+1 queries and make the data access pattern
implicit.

## Decision

OrderEntity and OrderItemEntity have no navigation properties. All related data is
loaded explicitly in the service layer with separate queries.

## Alternatives Considered

1. Include navigation properties with explicit `.Include()` calls — allows
   convenient eager loading but the `Include` is easy to forget, leading to N+1.
2. Lazy loading proxies — completely hides the query pattern; unacceptable for
   a system that needs predictable performance.

## Consequences

Positive:

- Query behaviour is always explicit and visible in the service layer.
- No accidental lazy-load queries in production.
- Entities are simple data containers — easy to reason about and test.

Tradeoffs:

- Loading an order with its items requires two queries instead of one with a join.
  Acceptable at current scale; revisit if query count becomes a bottleneck.

## Follow-Up

- None.
