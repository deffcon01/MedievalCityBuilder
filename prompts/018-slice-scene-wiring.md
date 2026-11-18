# Task 018: Simulation in the scene (vertical slice wiring)

> **Read `prompts/_RULES.md` first and follow it. It overrides anything below that conflicts with it.**

## Objective
Run `CitySimulation` inside the game so the player can draw roads, paint zones and watch houses, households and jobs appear over time. Presentation only: no new simulation rules.

## Read first
1. `AGENTS.md`, then `prompts/_RULES.md`
2. `docs/PROGRESS.md`: the Phase 2 domain task (P2a), the road tool, zone tool and save v2 sections
3. `src/Simulation/` (`CitySimulation`, `CitySnapshot`), `src/Presentation/Main.cs`, `TerrainView.cs`, `RoadTool.cs`, `ZoneTool.cs`
4. The building visuals code from task 012 (the presentation side)

## Allowed changes
- New `src/Presentation/SimulationDriver.cs`
- `src/Presentation/Main.cs`, `RoadTool.cs`, `ZoneTool.cs`: minimal changes (read-only accessors for the live `RoadGraph` and `ZoneGrid`)
- `docs/PROGRESS.md` (append-only). No `.tscn` edits. Do NOT check off TODO.md; the user does that after the F5 test.

## Requirements
1. `SimulationDriver` (a `Node` created in code from `Main.cs`): accumulates `_Process` delta time and calls `CitySimulation.Tick()` at a fixed interval (PLACEHOLDER 1.0 second; more than 5 ticks per frame is never run, extra time is dropped). The P key pauses and resumes. It logs one line on creation: `SimulationDriver: ready (P pauses)`.
2. The simulation references the LIVE road graph and zone grid. `RoadTool` and `ZoneTool` replace those objects when the world changes or on import, so `Main.cs` calls `SimulationDriver.Rebind(roadGraph, zoneGrid, ...)` explicitly after every `ApplyWorld`, `ImportRoads` and `ImportZones` (do not rely on the order of the `WorldApplied` event handlers). A rebind creates a new `CitySimulation` with the starting village and tick 0 (simulation state is saved only in task 019, so N and F7 restart it).
3. Debug output: every 10 ticks log `Sim tick=<t> pop=<n> households=<n> buildings=<n> jobs=<filled>/<total> homeless=<n>` (invariant culture) and show the same numbers in a small label layer.
4. Building visuals created by the task 012 code appear and are removed with the simulation. Add children to the tree BEFORE setting `GlobalPosition`.
5. Verification per `_RULES.md` (no new unit tests are expected; the test total must stay the same). Manual F5 checklist, marked "unverified manually": the starting village appears; draw a road and paint a residential zone next to it and houses appear over a few ticks; paint a commercial zone and workplaces appear and citizens get jobs; P pauses and resumes; the logged numbers match the label; N and F7 restart the simulation cleanly; no console errors, none containing `is_inside_tree`.

## Definition of done
- All verification steps in `_RULES.md` pass with real output.
- `docs/PROGRESS.md` updated by append only; TODO.md left unchanged.

## Stop conditions
- Stop and report when complete and verified.
- If the simulation cost per tick is too high to run on the main thread, report measurements instead of redesigning.

## Commit
`P2b: simulation scene wiring` (final commit; one commit per numbered item before it).
