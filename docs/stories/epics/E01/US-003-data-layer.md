# US-003 Data Layer — Entities, DbContext, Migration

## Status

planned

## Lane

normal

## Product Contract

OrderEntity and OrderItemEntity are persisted to MySQL via EF Core with correct
column types, UTC datetime handling, and decimal precision. A migration creates
the schema.

## Relevant Product Docs

- `docs/product/orders.md`

## Acceptance Criteria

- `OrderEntity` has fields: Id (long), CustomerId (long), Status (OrderStatus enum), TotalAmount (decimal), CreatedAt (DateTime), UpdatedAt (DateTime).
- `OrderItemEntity` has fields: Id (long), OrderId (long), ProductId (long), Quantity (int), Price (decimal).
- `OrderStatus` enum: Pending=1, Processing=2, Shipped=3, Delivered=4, Cancelled=5.
- `TotalAmount` and `Price` configured with `precision(18, 2)`.
- `CreatedAt` and `UpdatedAt` stored and loaded as UTC — a value converter normalizes both directions.
- No navigation properties on either entity.
- `DesignTimeDbContextFactory` allows EF CLI to run without a startup project at hand.
- Migration `InitialCreate` creates `Orders` and `OrderItems` tables with correct column types.
- `dotnet ef database update` succeeds against running MySQL.
- `AppDbContext_test`: 3 unit tests pass (round-trip, UTC kind, OrderItem fields).

## Design Notes

- Commands: `dotnet ef migrations add InitialCreate --project src/EventPipeline.Data --startup-project src/EventPipeline.Api`
- Queries: `db.Orders`, `db.OrderItems`
- API: none
- Tables: `Orders`, `OrderItems`
- Domain rules: no auto-timestamps; all DateTime values must be UTC
- UI surfaces: none

## Validation

| Layer | Expected proof |
| --- | --- |
| Unit | `AppDbContext_test` — EF mapping, UTC conversion, decimal round-trip (InMemory provider) |
| Integration | `dotnet ef database update` succeeds; `DESCRIBE Orders` shows correct column types |
| E2E | none |
| Platform | none |
| Release | none |

## Harness Delta

none

## Evidence
