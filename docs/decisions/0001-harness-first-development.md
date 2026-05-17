# 0001 Harness-First Development

Date: 2026-05-17

## Status

Accepted

## Context

EventPipeline is a non-trivial system involving multiple epics, external
infrastructure (Kafka, Debezium, Elasticsearch, ClickHouse), and concurrent
implementation across API, data, and worker layers.

A single large specification is not enough for safe agent work because it becomes
hard to locate current truth, risk, proof, and change history as the system grows.

## Decision

Establish a harness before scaffolding product code.

The harness defines:

- Agent entrypoint (`AGENTS.md`).
- Product contract split (`docs/product/`).
- Feature intake and risk lanes (`docs/FEATURE_INTAKE.md`).
- Story packet templates (`docs/templates/`).
- Decision records (`docs/decisions/`).
- Test matrix (`docs/TEST_MATRIX.md`).
- Harness backlog (`docs/HARNESS_BACKLOG.md`).

No application code is created until a selected story needs it.

## Consequences

Positive:

- Agents have a clear operating model before implementation starts.
- Product truth is split into small, maintainable files rather than one large spec.
- Risky work has a slower lane before code changes.
- Friction discovered during implementation is captured and improves future work.

Tradeoffs:

- Initial setup requires writing docs before writing code.
- Some harness files are generic until real stories exercise them.
