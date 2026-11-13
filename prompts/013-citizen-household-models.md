# Task 013: Citizen and household data models

## Objective

Implement the citizen and household domain data models — the first Phase 1.6 task. Data only, no simulation behavior yet.

## Read first

1. `AGENTS.md` — mandatory workflow
2. `DESIGN.md` — Section 15 "Population" (citizens, households, species: humans baseline, elves, dwarves), Section 16 "Citizen needs"
3. `src/Buildings/` — building entities (housing capacity context)
4. `src/Core/` — existing model patterns

## Allowed changes

- New `src/Population/` folder — models, pure C#
- `tests/MedievalCityBuilder.Tests.csproj` — include new source folder
- New tests
- `docs/PROGRESS.md`, `TODO.md`

## Requirements

1. Citizen: id, household, species, age, employment state, education state. Species is data-driven (a definition record with the traits from DESIGN.md Section 15), not an enum with baked-in behavior.
2. Household: id, members, home reference (nullable), wealth, needs state.
3. Needs: the planned needs from Section 16 (housing, food, education, warmth/fuel, employment, entertainment) as data with satisfaction state. Placeholder values only.
4. No behavior/simulation logic in this task — construction, validation, equality, and invariants only.
5. Tests: construction validation, invariants (citizen belongs to one household), species definitions, need state defaults.

## Definition of done

- `dotnet build` — 0 warnings, 0 errors; `dotnet test` — all pass
- `tools/run-headless.ps1` — unchanged, no WARNING/ERROR
- `docs/PROGRESS.md` updated; TODO.md "Citizen data model" and "Household data model" checked

## Stop conditions

- Stop and report when complete and verified.
- Do not implement household creation, growth, or housing placement logic.

## Commit

`P1-6a: citizen and household data models`
