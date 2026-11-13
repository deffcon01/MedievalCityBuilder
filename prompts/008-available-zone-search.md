# Task 008: Available-zone search

## Objective

Implement the search for developable zoned space: given a demand event, find the painted zone cells that pass all validation rules (minimum size, road distance, terrain) and are large enough for development.

## Read first

1. `AGENTS.md` — mandatory workflow
2. `DESIGN.md` — Section 6 "Parcel generation" steps 3-5, Section 7
3. `src/Simulation/` — demand events from task 007
4. `src/Zoning/` — grid and validation pipeline from tasks 003-005
5. `src/Roads/RoadGraph.cs`

## Allowed changes

- `src/Simulation/` — zone search code, pure C#
- New tests
- `docs/PROGRESS.md`, `TODO.md`

## Requirements

1. Given a zone type and demand, return candidate developable regions: connected cell groups of the requested zone type that pass all validations.
2. Return regions in a documented order (e.g., largest first, or nearest to road first) — deterministic.
3. Efficient enough for large maps: bounded work, no unbounded loops, early exit when enough candidates are found.
4. Tests: no valid zones, partially valid regions, ordering, multiple zone types, reproducibility with seeds.

## Definition of done

- `dotnet build` — 0 warnings, 0 errors; `dotnet test` — all pass
- `tools/run-headless.ps1` — unchanged, no WARNING/ERROR
- `docs/PROGRESS.md` updated; TODO.md "Available-zone search" checked

## Stop conditions

- Stop and report when complete and verified.
- Do not implement parcel fitting or building selection.

## Commit

`P1-5b: available-zone search`
