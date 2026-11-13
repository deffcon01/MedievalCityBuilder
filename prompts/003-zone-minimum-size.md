# Task 003: Zone minimum size validation

## Objective

Implement minimum zone size validation for painted zones. When a player paints a zone, each connected painted region of a zone type must be at least a minimum number of cells to count as a valid zone; regions below the minimum are rejected (not painted / erased).

This is a pure domain-logic task: no Godot scene code.

## Read first

1. `AGENTS.md` — mandatory workflow
2. `DESIGN.md` — Section 5 "Zoning": "Each zone has minimum sizing rules, maximum distance from a valid path/road, terrain suitability requirements"
3. `ARCHITECTURE.md` — data-driven rule, testing section
4. `docs/PROGRESS.md` — latest entries (P1-4a section)
5. `src/Zoning/ZoneGrid.cs` and `src/Zoning/ZoneRegistry.cs` — existing grid/brush
6. `tests/MedievalCityBuilder.Tests/ZoneGridTests.cs` — existing test style

## Allowed changes

- New file `src/Zoning/ZoneValidation.cs` (or similar) — pure C#, no Godot types
- New file `tests/MedievalCityBuilder.Tests/ZoneValidationTests.cs`
- `src/Zoning/ZoneRegistry.cs` / `ZoneDefinition.cs` — only if adding a `MinSizeCells` field is needed
- `docs/PROGRESS.md` — append entry
- `TODO.md` — check off "Minimum zone size"

## Requirements

1. Zone definitions must carry a minimum size (cells). Exact default value is a prototype placeholder — make it configurable, do not treat it as final balance.
2. Validation must operate on the grid: given painted cells, identify connected regions of the same zone type (4-neighbor connectivity), and flag regions below the minimum.
3. Provide a method that paints a brush stroke through validation: cells that would create an undersized region are either rejected or painted-then-cleaned — pick one approach, document it in the code, and test it.
4. No hard-coded zone lists in validation logic (data-driven rule).
5. Deterministic: same input, same result. Use no System.Random.
6. Tests: connected-region detection, minimum-size boundary (exactly at min = valid, min-1 = invalid), multiple regions in one stroke, reproducibility.

## Definition of done

- `dotnet build MedievalCityBuilder.csproj` — 0 warnings, 0 errors
- `dotnet test` — all tests pass (existing 468 + new)
- `tools/run-headless.ps1` — unchanged expected output, no WARNING/ERROR
- `docs/PROGRESS.md` updated with what/commands/results
- TODO.md checkbox updated

## Stop conditions

- Stop and report when the task is complete and verified.
- If a design decision is required that DESIGN.md does not cover (e.g., whether undersized paint should be rejected or auto-erased), stop and report the options instead of guessing.
- Do not implement road-distance or terrain validation in this task.

## Commit

One or two small commits, message: `P1-4c: zone minimum size validation`
