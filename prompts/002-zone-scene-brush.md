# Task 002: Zone painting scene brush

## Objective

Implement the in-game zone painting tool: the player selects a zone type and paints cells on the terrain with the mouse, using the existing ZoneGrid domain code.

## Read first

1. `AGENTS.md` — mandatory workflow
2. `DESIGN.md` — Section 5 "Zoning" (paint-brush system, zone list)
3. `docs/PROGRESS.md` — P1-4a section
4. `src/Zoning/ZoneGrid.cs`, `src/Zoning/ZoneRegistry.cs`
5. `src/Presentation/RoadTool.cs` — existing tool pattern (toggle key, mouse handling)
6. `src/Presentation/TerrainView.cs`, `src/World/TerrainRaycaster.cs` — mouse-to-world raycasting

## Allowed changes

- New file `src/Presentation/ZoneTool.cs` (follow RoadTool pattern)
- `scenes/Main.tscn` — wire the tool node
- `src/Presentation/Main.cs` — minimal wiring only
- New tests for any new pure logic
- `docs/PROGRESS.md`, `TODO.md`

## Requirements

1. Tool toggled by a key (e.g. Z). While active, left-click paints the selected zone type, right-click (or shift-click) erases.
2. Zone type selection: cycle with a key or number keys 1-8 using the starting registry definitions. Keep the UI minimal — a console/log line listing the selected zone is enough for the prototype.
3. Brush radius is a configurable placeholder.
4. Painting must go through ZoneGrid.Paint/Erase — no duplicate grid logic in the presentation layer.
5. Visual feedback: tinted cells or simple markers over painted cells. Placeholder quality is fine.
6. Input behavior cannot be verified headless: include a manual F5 checklist in PROGRESS.md, like the camera tasks.

## Definition of done

- `dotnet build` — 0 warnings, 0 errors
- `dotnet test` — all pass
- `tools/run-headless.ps1` — expected lines present (new ZoneTool ready line allowed), no WARNING/ERROR
- `docs/PROGRESS.md` updated with manual test checklist; TODO.md "Zone painting" checked

## Stop conditions

- Stop and report when complete and verified.
- If terrain raycasting cannot support cell-level precision, stop and report options.

## Commit

`P1-4b: zone painting scene brush`
