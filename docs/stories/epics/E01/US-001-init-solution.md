# US-001 Initialize Solution and Projects

## Status

implemented

## Lane

normal

## Product Contract

A developer can create a .NET 10 solution containing four projects (Api, Data,
Services, Worker) and one test project with correct project references and a
pinned SDK version.

## Relevant Product Docs

- `docs/product/overview.md`

## Acceptance Criteria

- Solution file `EventPipeline.sln` exists at repo root.
- `global.json` pins SDK to `10.0.100` with `latestPatch` roll-forward.
- Four source projects and one test project exist under `src/` and `test/`.
- Project references match: Api → Data + Services; Services → Data; Worker → Data; Tests → Data + Services + Worker.
- `dotnet build` succeeds with no errors.

## Design Notes

- Commands: `dotnet new sln`, `dotnet new webapi/classlib/worker/xunit`, `dotnet sln add`, `dotnet add reference`
- Queries: none
- API: none
- Tables: none
- Domain rules: none
- UI surfaces: none

## Validation

| Layer | Expected proof |
| --- | --- |
| Unit | none |
| Integration | `dotnet build` succeeds |
| E2E | none |
| Platform | none |
| Release | none |

## Harness Delta

none

## Evidence

- `dotnet build` → `Build succeeded. 0 Warning(s) 0 Error(s)`
- Solution: `EventPipeline.sln` with 5 projects
- References: Api→Data+Services, Services→Data, Worker→Data, Tests→Data+Services+Worker
