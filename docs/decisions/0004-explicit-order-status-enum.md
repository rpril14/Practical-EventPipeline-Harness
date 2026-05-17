# 0004 Explicit Order Status Enum

Date: 2026-05-17

## Status

Accepted

## Context

The order entity needs a way to represent lifecycle state. The options were a
closed enum, a string column, or a free-form label table. The worker uses status
to fan out to Elasticsearch and ClickHouse, and future filtering and overdue
logic will depend on predictable state transitions.

## Decision

Use a closed integer enum (`OrderStatus`) with five values: Pending=1, Processing,
Shipped, Delivered, Cancelled. Adding a new status value is a breaking data model
change requiring a migration and a new decision record.

## Alternatives Considered

1. Free-form string column — flexible but validation must be enforced at every
   boundary; harder to guarantee consistency across service and worker.
2. Status lookup table — normalised but adds a join and a migration on every
   status addition; overkill for a closed set.

## Consequences

Positive:

- Status is always valid at the type level.
- No string comparison bugs in handler routing.
- Filtering and analytics queries are efficient (integer column).

Tradeoffs:

- Adding a new status requires a code change and a migration, not just a data
  insert.

## Follow-Up

- If future product work adds an `Overdue` or `Returned` status, open a change
  request and update this decision.
