# Agent Operating Guide

## Source Of Truth

Read in this order:

1. `README.md` for project status.
2. `docs/HARNESS.md` for the human-agent operating model.
3. `docs/FEATURE_INTAKE.md` before turning any prompt into work.
4. The user-provided spec or prompt, when one exists.
5. `docs/product/` for current product contracts.
6. `docs/ARCHITECTURE.md` before proposing implementation shape.
7. `docs/stories/` for story packets and backlog.
8. `docs/TEST_MATRIX.md` for proof status.
9. `docs/decisions/` for why important choices were made.

This harness does not ship with a project-specific `SPEC.md`. When the human
provides a spec for a new project, treat that spec as input material for the
first buildout. Derive product docs, story packets, architecture decisions, and
validation expectations from it. Product docs, stories, tests, and decisions
then become the living contract that agents should update as the system evolves.

## Session Rules

These rules apply to every session, without exception:

1. **Never commit or push without explicit human approval.** Make all changes,
   show a summary of what changed, and wait for the human to say it is ready
   before running `git commit` or `git push`.

2. **Update relevant docs immediately when code or business logic changes.**
   This applies the moment a change is made — not deferred to the end of the
   task. This includes self-corrections: if the agent fixes a bug, adjusts
   behavior, or revises something during implementation, the corresponding docs
   must be updated at that point, not accumulated for later.

   Which file to update depends on what changed:
   - `docs/product/` — behavior contract, API shape, business rules.
   - `docs/ARCHITECTURE.md` — solution structure, stack, layer or boundary rules.
   - `docs/stories/` — story status and evidence.
   - `docs/TEST_MATRIX.md` — proof status and evidence.
   - `docs/decisions/` — an architecture or product choice was made or changed.
   - `docs/HARNESS_BACKLOG.md` — friction was discovered.

## Task Loop

For every task:

1. Classify the request with `docs/FEATURE_INTAKE.md`.
2. Identify whether the input is a new spec, spec slice, change request, new
   initiative, maintenance request, or harness improvement.
3. Locate the affected product docs and story files.
4. Check `docs/TEST_MATRIX.md` for existing proof and gaps.
5. Work only inside the selected lane: tiny, normal, or high-risk.
6. Before finishing, ask:
   - Did product truth change?
   - Did validation expectations change?
   - Did architecture rules change?
   - Did we discover a repeated failure pattern?
   - Did the next agent need a clearer instruction?
7. Update routine harness files directly, or add a proposal to
   `docs/HARNESS_BACKLOG.md` when the change is structural.
8. Show a summary of all changes and wait for human approval before committing.

## Harness Change Policy

Agents may update directly:

- Story status and evidence.
- `docs/TEST_MATRIX.md` rows.
- Links from story packets to product docs.
- Validation notes and reports.
- Small clarifications tied to the current task.

Agents should ask for human confirmation before:

- Changing architecture direction.
- Removing validation requirements.
- Changing the source-of-truth hierarchy.
- Changing risk classification rules.
- Replacing the feature workflow.

## Done Definition

A task is done only when:

- The requested change is completed or the blocker is documented.
- Relevant docs, stories, and test matrix entries remain current.
- Validation commands were run when they exist.
- Missing harness capabilities were added to `docs/HARNESS_BACKLOG.md`.
- The human has reviewed and approved the changes.
- `git commit` and `git push` have been run only after that approval.
- The final response says what changed and what was not attempted.
