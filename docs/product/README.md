# Product Docs

Product contracts for the EventPipeline project, derived from the original spec.

- `overview.md`: system purpose, data flow, and tech stack.
- `orders.md`: order entity contract, status enum, and API contract.
- `pipeline.md`: CDC event shape, worker behavior, retry policy, and DLQ.

## Update Rule

When behavior changes:

1. Update the affected product doc.
2. Update or create the story packet.
3. Update `docs/TEST_MATRIX.md`.
4. Record a decision if the change affects architecture, scope, risk, or a
   previously settled product rule.
