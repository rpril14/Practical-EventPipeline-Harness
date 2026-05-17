# 0007 Commit Kafka Offset After DLQ Routing

Date: 2026-05-17

## Status

Accepted

## Context

When a Kafka consumer fails to process a message, it has two choices: commit the
offset (move past the message) or not commit (retry on restart). Not committing
causes the same message to be redelivered indefinitely on restart, creating an
infinite loop for poison messages.

## Decision

The consumer always commits the offset after routing a message to the DLQ,
regardless of whether processing succeeded or failed. This prevents infinite retry
loops. The DLQ topic holds the raw message for manual inspection and reprocessing.

## Alternatives Considered

1. Do not commit on failure — guarantees at-least-once delivery but causes infinite
   loops on poison messages unless a separate dead-letter mechanism is in place.
2. Commit only after max retries — same outcome as the chosen approach but the
   commit is implicit inside RetryPolicy, mixing concerns.

## Consequences

Positive:

- No partition stall or consumer group lag caused by a single poison message.
- DLQ provides a recoverable record for manual reprocessing.

Tradeoffs:

- If the DLQ produce fails (e.g. Kafka is down), the message is lost. Acceptable
  risk for this system; add a local fallback log if durability requirements
  increase.

## Follow-Up

- Add monitoring on the DLQ topic to alert on accumulation.
