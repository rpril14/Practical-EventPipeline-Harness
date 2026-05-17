# Glossary

## Harness Terms

### Agent

An AI coding collaborator operating inside the repository.

### Harness

The repo-level operating system that tells humans and agents how to turn intent
into safe product changes.

### Product Contract

The current expected behavior of the product. Product docs plus executable tests
become the living contract once implementation exists.

### Story Packet

A story-sized work file or folder that describes the product contract, affected
docs, design notes, and validation expectations for a feature.

### Feature Intake

The classification step that turns a prompt into tiny, normal, or high-risk
work before implementation begins.

### Harness Delta

A documentation, template, validation, backlog, or decision update that makes
future agent work safer or easier.

### Product Delta

A product-facing change such as code, tests, API shape, data model, or product
documentation.

---

## Project Terms

### CDC (Change Data Capture)

The mechanism by which Debezium reads the MySQL binary log and converts row-level
changes into structured events published to Kafka.

### Debezium Envelope

The JSON wrapper Debezium adds around every CDC event:
`{"schema":{...},"payload":{"before":..., "after":..., "op":"c|u|d|r", "ts_ms":...}}`.
The consumer extracts `payload` before deserializing into `CdcEvent<T>`.

### CdcEvent

The typed model representing a single Debezium CDC event. Generic over the row
snapshot type (`CdcEvent<OrderSnapshot>`). Fields: `Before`, `After`, `Op`, `TsMs`.

### OrderSnapshot

The typed model representing one row from the `Orders` table as delivered by
Debezium. Field names are PascalCase matching EF Core column names. `TotalAmount`
arrives as a JSON number (`decimal.handling.mode=double`). `CreatedAt` and
`UpdatedAt` arrive as microseconds since Unix epoch.

### op

The Debezium operation code in a CDC event:
- `c` — row created
- `u` — row updated
- `d` — row deleted
- `r` — snapshot read (initial sync on connector start)

### DLQ (Dead Letter Queue)

The Kafka topic (`ecommerce.orders_db.Orders.dlq`) where the Worker publishes
messages that failed after all retry attempts or encountered a permanent error.
The offset is always committed after DLQ routing to prevent infinite loops.

### RetryPolicy

The Worker component that wraps handler execution with exponential backoff (1s,
2s, 4s, 8s, 16s, max 5 attempts). Retries on transient exceptions
(`HttpRequestException`, `SocketException`, `TimeoutException`, `IOException`).
Routes to DLQ immediately on permanent exceptions.
