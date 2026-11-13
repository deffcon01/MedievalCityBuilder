# Task 007: Development demand events

## Objective

Implement the simulation's development demand events: when the simulation needs a building (new household needs housing, business opens, demand increases), it raises a demand event that the building development system will later consume.

## Read first

1. `AGENTS.md` — mandatory workflow
2. `DESIGN.md` — Section 7 "Development events" and Section 5 zoning context
3. `ARCHITECTURE.md` — simulation timing, data-driven rule
4. `src/Zoning/` — everything (grid, registry, validation)
5. `src/Core/` — existing domain patterns

## Allowed changes

- New folder/files `src/Simulation/` — pure C# domain code
- New tests `tests/MedievalCityBuilder.Tests/SimulationTests.cs` (or similar)
- `tests/MedievalCityBuilder.Tests/MedievalCityBuilder.Tests.csproj` — include new source folder
- `docs/PROGRESS.md`, `TODO.md`

## Requirements

1. Demand event types: housing demand (household count), commercial demand, industrial demand — data-driven, extensible via definitions, not hard-coded switch statements on strings.
2. A demand queue/collector that records events with: type, zone category needed, timestamp/tick, and cause.
3. Simulation tick concept: a simple counter-based tick the events reference. Do not build the full simulation loop — just the event model and queue.
4. Events must be deterministic and testable without Godot.
5. Tests: event creation, queue ordering, deduplication policy (document it), reproducibility.

## Definition of done

- `dotnet build` — 0 warnings, 0 errors; `dotnet test` — all pass
- `tools/run-headless.ps1` — unchanged, no WARNING/ERROR
- `docs/PROGRESS.md` updated; TODO.md "Development demand event" checked

## Stop conditions

- Stop and report when complete and verified.
- Do not implement zone search, parcel fitting, or building placement — later tasks.

## Commit

`P1-5a: development demand events`
