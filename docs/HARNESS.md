# Harness

The app is what users touch. The harness is what agents touch.

## Mental Model

```text
+------------------+
| Human intent     |
+------------------+
         |
         v
+------------------+
| Feature intake   |
+------------------+
         |
         v
+------------------+
| Story packet     |
+------------------+
         |
         v
+------------------+
| Agent work loop  |
+------------------+
         |
         v
+------------------+
| Product delta    |
+------------------+
         |
         v
+------------------+
| Validation proof |
+------------------+
         |
         v
+------------------+
| Harness delta    |
+------------------+
         |
         v
+------------------+
| Next intent      |
+------------------+
```

Every task has two possible outputs:

1. Product delta: app code, tests, API shape, data model, or product docs.
2. Harness delta: docs, templates, validation expectations, backlog items, or
   decision records that make the next task easier.

## Source Hierarchy

```text
docs/product/*
  current product contract

docs/ARCHITECTURE.md
  solution structure, tech stack, layer responsibilities, boundary rules

docs/stories/*
  story-sized work packets and historical evidence

docs/TEST_MATRIX.md
  behavior-to-proof control panel

docs/decisions/*
  why the contract or architecture changed
```

Before implementation, product docs describe intent. After implementation,
product docs plus executable tests become the living contract.

## Spec Lifecycle

When the human provides a specification, treat it as input material, not as a
permanent operating manual. Use it to populate product docs, story packets,
architecture decisions, and validation expectations during the first buildout.

After the specification has been decomposed, do not keep extending it as the
living product plan. Ongoing work should update the smaller product docs,
stories, test matrix, and decision records.

Ongoing work should enter the harness as one of these input types:

- New spec: a project specification that needs to become product docs and
  initial story candidates.
- Spec slice: a selected behavior from the provided spec.
- Change request: a bounded behavior change, bug fix, or product refinement.
- New initiative: a larger product area that needs multiple stories.
- Maintenance request: dependency, architecture, performance, security, or
  operational work.
- Harness improvement: a process, template, proof, or agent-instruction change.

The spec-to-work loop is:

```text
human intent or supplied spec
  -> classify input type
  -> update or create product contract
  -> create story packet or initiative notes when needed
  -> define validation proof
  -> implement or document the blocker
  -> update product docs, stories, test matrix, and decisions
  -> capture harness friction
```

## Growth Rule

The harness grows from friction.

When an agent is confused, repeats manual reasoning, needs a new validation
command, discovers a missing rule, or sees a recurring failure pattern, it must
either improve the harness directly or add a proposal to `HARNESS_BACKLOG.md`.

## Validation Ladder

```bash
# Quick checks — run before every commit
dotnet test

# Integration — requires Docker infrastructure running
cd scripts && docker compose up -d
dotnet ef database update --project src/EventPipeline.Data --startup-project src/EventPipeline.Api

# E2E — requires connector registered and Worker running
curl -X POST http://localhost:5000/orders ...
# verify document in Elasticsearch and ClickHouse
```

Agents must not claim these commands pass until they have been run and output
confirmed.
