# Orders

## Entities

### Order

| Field | Type | Rules |
| --- | --- | --- |
| Id | long | Auto-generated primary key |
| CustomerId | long | Required, set at creation |
| Status | OrderStatus (enum) | Starts as Pending; transitions via API |
| TotalAmount | decimal(18,2) | Computed from items at creation: `sum(quantity × price)` |
| CreatedAt | DateTime (UTC) | Set manually in service layer at creation |
| UpdatedAt | DateTime (UTC) | Set manually in service layer at creation and on every status update |

No navigation properties. No auto-timestamps from EF.

### OrderItem

| Field | Type | Rules |
| --- | --- | --- |
| Id | long | Auto-generated primary key |
| OrderId | long | Foreign key to Order |
| ProductId | long | Required |
| Quantity | int | Required |
| Price | decimal(18,2) | Unit price at time of order |

## Order Status

```
Pending = 1
Processing = 2
Shipped = 3
Delivered = 4
Cancelled = 5
```

Status set is closed. Adding a new status value is a breaking data model change
and requires a migration and a decision record.

## API Contract

### POST /orders

Creates an order and its items.

Request:
```json
{
  "customerId": 1,
  "items": [
    { "productId": 10, "quantity": 2, "price": 15.00 }
  ]
}
```

Response: `201 Created` with the created order including computed `totalAmount`.

### PUT /orders/{id}/status

Updates the order status.

Request:
```json
{ "status": 2 }
```

Response: `200 OK` with updated order, or `404 Not Found`.

### GET /orders/{id}

Returns the order with its items.

Response: `200 OK` with order + items, or `404 Not Found`.

## Validation Rules

- `TotalAmount` is always recomputed at creation — never accepted from the client.
- `CreatedAt` and `UpdatedAt` are always stored and read back as UTC.
- Decimal columns use `precision(18, 2)`.
