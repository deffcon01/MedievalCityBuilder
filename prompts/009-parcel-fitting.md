# Task 009: Parcel fitting (footprints, fallback, leftovers)

## Objective

Implement parcel fitting: given a developable region and building footprint requirements, fit the largest eligible building footprint, with the largest-fitting fallback and leftover-area assignment rules.

## Read first

1. `AGENTS.md` — mandatory workflow
2. `DESIGN.md` — Section 6 "Parcel generation" steps 4-8 (especially: if the building does not fit, choose the largest eligible that fits; if remaining area is too small, assign it to the last generated building)
3. `src/Simulation/` — demand events, zone search (tasks 007-008)
4. `src/Zoning/ZoneGrid.cs`

## Allowed changes

- `src/Simulation/` — parcel fitting code, pure C#
- New tests
- `docs/PROGRESS.md`, `TODO.md`

## Requirements

1. Footprint model: width/depth in cells, with a small set of placeholder footprint classes (small/medium/large). Exact dimensions are prototype placeholders — configurable.
2. Fit algorithm: try the requested footprint; if it does not fit the region, try the largest eligible smaller footprint; if remaining cells are too small for any valid building, attach them to the last parcel.
3. Output: parcel rectangles (cell coordinates + footprint class) that consume the region without overlap.
4. Deterministic; no unbounded search; documented tie-breaking.
5. Tests: exact fit, fallback to smaller, leftover assignment, multiple parcels from one region, region too small entirely, reproducibility.

## Definition of done

- `dotnet build` — 0 warnings, 0 errors; `dotnet test` — all pass
- `tools/run-headless.ps1` — unchanged, no WARNING/ERROR
- `docs/PROGRESS.md` updated; TODO.md "Building footprint/parcel fitting", "Largest-fitting-building fallback", "Leftover-area assignment" checked

## Stop conditions

- Stop and report when complete and verified.
- Do not implement building asset selection or placement.

## Commit

`P1-5c: parcel fitting with largest-fitting fallback and leftover assignment`
