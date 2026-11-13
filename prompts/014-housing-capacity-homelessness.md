# Task 014: Housing capacity, household placement, homelessness

## Objective

Implement housing capacity tracking, household creation, placement of households into homes, and homelessness handling.

## Read first

1. `AGENTS.md` — mandatory workflow
2. `DESIGN.md` — Section 15, Section 16 (housing need), Section 3 (starting village 8-12 citizens)
3. `src/Population/` — models from task 013
4. `src/Buildings/` — building entities, asset capacity field
5. `src/Simulation/` — demand events (housing demand exists from task 007)

## Allowed changes

- `src/Population/` and/or `src/Simulation/` — housing logic, pure C#
- New tests
- `docs/PROGRESS.md`, `TODO.md`

## Requirements

1. Housing capacity: sum of residential building capacity; track occupancy per building.
2. Household creation: deterministic creation driven by housing demand events, using seeded randomness for household size (placeholder ranges, configurable).
3. Placement: household moves into a home with free capacity; capacity never exceeded (enforce invariant).
4. Homelessness: households with no home are homeless; homelessness state queryable for the happiness system later.
5. Tests: capacity math, placement, over-capacity rejection, homeless household, household creation determinism, full-house scenario.

## Definition of done

- `dotnet build` — 0 warnings, 0 errors; `dotnet test` — all pass
- `tools/run-headless.ps1` — unchanged, no WARNING/ERROR
- `docs/PROGRESS.md` updated; TODO.md "Housing capacity", "Household creation", "Household placement into homes", "Homelessness" checked

## Stop conditions

- Stop and report when complete and verified.
- Do not implement employment, wealth, or happiness.

## Commit

`P1-6b: housing capacity, household placement, homelessness`
