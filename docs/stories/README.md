# Stories

Stories are work packets. They turn product intent into bounded implementation
and validation work.

See `backlog.md` for the full list of stories across 4 epics (E01–E04).

## Normal Story

Use `docs/templates/story.md` for normal feature work.

Path convention:

```text
docs/stories/epics/E01-domain-name/US-001-short-story-title.md
```

## High-Risk Story

Use `docs/templates/high-risk-story/` when the feature intake classifies work as
high-risk.

Path convention:

```text
docs/stories/epics/E02-risky-domain/US-012-risky-story-title/
  execplan.md
  overview.md
  design.md
  validation.md
```

## Status Flow

```text
planned -> in_progress -> implemented
                |
                v
             changed
                |
                v
             retired
```
