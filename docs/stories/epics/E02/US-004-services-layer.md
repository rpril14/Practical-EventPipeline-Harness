# US-004 Services Layer — OrderService

## Status

implemented

## Lane

normal

## Product Contract

OrderService exposes three operations: create an order (with item persistence and
totalAmount computation), update order status, and retrieve an order with its items.

## Relevant Product Docs

- `docs/product/orders.md`

## Acceptance Criteria

- `CreateAsync` computes `TotalAmount = sum(quantity × price)` from the request items.
- `CreateAsync` sets `Status = Pending`, `CreatedAt = UtcNow`, `UpdatedAt = UtcNow`.
- `CreateAsync` persists the order and all items in a single logical operation.
- `UpdateStatusAsync` updates `Status` and `UpdatedAt`; returns null if order not found.
- `GetAsync` returns the order with all its items; returns null if not found.
- `IOrderService` interface is defined separately from the implementation.
- `OrderService_test`: 7 unit tests pass.

## Design Notes

- Commands: `db.Orders.Add`, `db.OrderItems.AddRange`, `db.SaveChangesAsync`
- Queries: `db.Orders.FindAsync`, `db.OrderItems.Where(i => i.OrderId == id).ToListAsync`
- API: none (called by controller)
- Tables: Orders, OrderItems
- Domain rules: TotalAmount is computed, never accepted from client
- UI surfaces: none

## Validation

| Layer | Expected proof |
| --- | --- |
| Unit | `OrderService_test` — 7 tests covering create, update, get, null cases (InMemory provider) |
| Integration | none |
| E2E | none |
| Platform | none |
| Release | none |

## Harness Delta

none

## Evidence

- `OrderService_test` → 8 passed
