# Task 012: Building placement

## Objective

Implement building placement: when a parcel has a selected asset, create the building entity (domain state), place it in the world, and present a placeholder visual. This completes Phase 1.5.

## Read first

1. `AGENTS.md` — mandatory workflow
2. `DESIGN.md` — Section 6 step 8 "Develop the building", Section 10 (buildings do not expand)
3. `src/Simulation/` — demand, zone search, parcels, selection (tasks 007-011)
4. `src/Buildings/` — asset definitions
5. `src/Presentation/TerrainView.cs`, `scenes/Main.tscn` — how visuals are wired

## Allowed changes

- `src/Buildings/` — building entity/state, pure C# placement logic
- `src/Simulation/` — orchestration from demand event to placed building
- `src/Presentation/` — minimal placeholder visual (simple box/marker, no final art)
- New tests
- `docs/PROGRESS.md`, `TODO.md`

## Requirements

1. Building entity: id, asset, parcel, position, architectural generation — plain data, no Godot types.
2. Placement pipeline: demand event -> zone search -> parcel fit -> asset selection -> building entity. One call from the simulation side; presentation subscribes/applies visuals.
3. Placeholder visual only: a simple mesh/marker per building. Follow the data-driven rule — no asset lists in logic.
4. Placed buildings must be persistable-ready (plain data) but do not add save format changes in this task.
5. Tests: full pipeline test from demand to placed building, determinism (same seed same city), multiple buildings, no valid zone -> no building, no crash.

## Definition of done

- `dotnet build` — 0 warnings, 0 errors; `dotnet test` — all pass
- `tools/run-headless.ps1` — unchanged or extended with new expected lines, no WARNING/ERROR
- `docs/PROGRESS.md` updated; TODO.md "Building placement" checked — Phase 1.5 complete

## Stop conditions

- Stop and report when complete and verified.
- Do not add population, households, or employment — Phase 1.6 tasks.

## Commit

`P1-5f: building placement with placeholder visuals — Phase 1.5 complete`
