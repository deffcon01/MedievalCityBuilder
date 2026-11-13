# Task 001: Road entrance snapping

## Objective

Implement road entrance snapping: when the player draws a road close to a building entrance, the road path snaps to the entrance point.

## Read first

1. `AGENTS.md` — mandatory workflow
2. `DESIGN.md` — Section 11 "Roads": roads "automatically snap to building entrances when close"
3. `docs/PROGRESS.md` — Phase 1.3 sections
4. `src/Roads/` — RoadGraph, RoadPolyline, RoadPlacementController, RoadPlacementValidator
5. `src/Presentation/RoadTool.cs` — player road drawing

## Allowed changes

- `src/Roads/` — snapping logic as pure C# domain code
- New tests in `tests/MedievalCityBuilder.Tests/`
- `src/Presentation/RoadTool.cs` — only the minimal wiring needed
- `docs/PROGRESS.md`, `TODO.md`

## Requirements

1. Snapping is pure domain logic: given a candidate road endpoint and building entrance points, return the snapped point if within a configurable snap radius.
2. Snap radius is a prototype placeholder — configurable, not final.
3. Buildings do not exist yet; define a minimal entrance-point interface/record the road system can consume without depending on a future building system.
4. Tests: snap within radius, no snap outside radius, nearest entrance wins, tie behavior documented, radius 0 disables snapping.
5. Deterministic; no System.Random.

## Definition of done

- `dotnet build MedievalCityBuilder.csproj` — 0 warnings, 0 errors
- `dotnet test` — all pass
- `tools/run-headless.ps1` — unchanged expected output, no WARNING/ERROR
- `docs/PROGRESS.md` updated; TODO.md "Entrance snapping" checked

## Stop conditions

- Stop and report when complete and verified.
- If the entrance-point model risks conflicting with the future building system, stop and report instead of guessing.

## Commit

`P1-3d: road entrance snapping`
