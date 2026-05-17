# US-005 API Layer — OrdersController

## Status

implemented

## Lane

normal

## Product Contract

The REST API exposes three endpoints for orders. Each endpoint delegates to
IOrderService and returns the correct HTTP status code.

## Relevant Product Docs

- `docs/product/orders.md`

## Acceptance Criteria

- `POST /orders` returns `201 Created` with the created order body.
- `PUT /orders/{id}/status` returns `200 OK` with updated order, or `404 Not Found`.
- `GET /orders/{id}` returns `200 OK` with order + items, or `404 Not Found`.
- `AppDbContext` and `IOrderService` are registered in `Program.cs`.
- MySQL connection string is read from `ConnectionStrings:Default` in `appsettings.json`.
- Smoke test: `curl POST /orders` → `curl GET /orders/{id}` returns the created order.

## Design Notes

- Commands: POST /orders, PUT /orders/{id}/status
- Queries: GET /orders/{id}
- API: `[ApiController]`, `[Route("orders")]`, primary constructor injection
- Tables: via IOrderService
- Domain rules: controller is a thin pass-through; no business logic here
- UI surfaces: none

## Validation

| Layer | Expected proof |
| --- | --- |
| Unit | none — controller logic is trivial pass-through |
| Integration | none |
| E2E | `curl POST /orders` → `curl GET /orders/{id}` returns created order |
| Platform | none |
| Release | none |

## Harness Delta

none

## Evidence

- `dotnet build src/EventPipeline.Api` → succeeded
- Full test suite → 11 passed, 0 failed
- E2E smoke test pending: requires MySQL running
