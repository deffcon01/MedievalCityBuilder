# Task 004: Zone maximum distance from road

## Objective

Implement the zone placement rule that painted zone cells must be within a maximum distance of a valid road/path to be developable.

## Read first

1. `AGENTS.md` — mandatory workflow
2. `DESIGN.md` — Section 5 "Zoning": maximum distance from a valid path/road
3. `src/Roads/RoadGraph.cs` — existing road network
4. `src/Zoning/ZoneGrid.cs`
5. Task 003 output (`src/Zoning/` validation code) — reuse its patterns

## Allowed changes

- `src/Zoning/` — new validation code, pure C#
- New tests
- `docs/PROGRESS.md`, `TODO.md`

## Requirements

1. Given the road network and a zone cell, compute whether the cell is within the maximum distance of a road. Distance metric choice (straight-line vs path) must be documented; straight-line is acceptable for the prototype.
2. Max distance is a configurable placeholder.
3. Provide a query the future building-development system can use: "which painted cells are currently developable?" — do not build the development system itself.
4. Tests: cell adjacent to road, cell at exact limit, cell beyond limit, no roads at all, reproducibility.
5. Pure domain code, no Godot types, deterministic.

## Definition of done

- `dotnet build` — 0 warnings, 0 errors; `dotnet test` — all pass
- `tools/run-headless.ps1` — unchanged, no WARNING/ERROR
- `docs/PROGRESS.md` updated; TODO.md "Maximum distance from road/path" checked

## Stop conditions

- Stop and report when complete and verified.
- Do not implement building development in this task.

## Commit

`P1-4e: zone road distance validation`
