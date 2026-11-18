# Task 017: Save v2 scene wiring (roads and zones)

> **Read `prompts/_RULES.md` first and follow it. It overrides anything below that conflicts with it.**

## Objective
Make the F6 (save) and F7 (load) keys store and restore the placed roads and the painted zones, using the v2 save format from task 006. After this task a saved world reloads with its terrain, roads and zones.

## Read first
1. `AGENTS.md`, then `prompts/_RULES.md`
2. `docs/PROGRESS.md`: the F6/F7/N key wiring and the P1-4g (save v2) sections
3. `src/Presentation/Main.cs`, `src/Presentation/TerrainView.cs`, `src/Presentation/RoadTool.cs`, `src/Presentation/ZoneTool.cs`
4. `src/Persistence/` (`WorldSaveData`, `RoadSaveData`, `ZoneSaveData`, `WorldSaveFile`)

## Allowed changes
- `src/Presentation/RoadTool.cs`, `src/Presentation/ZoneTool.cs`, `src/Presentation/Main.cs`
- `docs/PROGRESS.md` (append-only), `TODO.md` ("Zone persistence" only, and only after the user confirms the manual test)
- Do NOT edit any `.tscn` file. Do NOT change `src/Persistence`, `src/Roads`, `src/Zoning` or any test.

## Requirements
1. `RoadTool`: add `ExportRoads()` (returns `RoadSaveData.FromGraph` of the live graph) and `ImportRoads(RoadSaveData)` which rebuilds the graph with `ToGraph()`, creates a new `RoadPlacementController` on it (no pending start), and rebuilds ALL visuals: one node marker per node and one ribbon per segment, reusing the existing ribbon-building code (refactor `AddSegmentVisuals` so it can work from segment and node ids). Import must not throw for a valid save; if a node lies outside the current terrain surface or a segment is shorter than 1e-9 units, log one clear `GD.PrintErr` line and import nothing.
2. `ZoneTool`: add `ExportZones()` (`ZoneSaveData.FromGrid`) and `ImportZones(ZoneSaveData)` (`ToGrid()`), rebuilding the overlay. Any failure logs one `GD.PrintErr` line and leaves the empty grid.
3. `Main.cs`: F6 builds `WorldSaveData` from `TerrainView.CurrentWorld` with `Roads` and `Zones` set from the two tools, then saves exactly as before. F7 loads, calls `TerrainView.ApplyWorld(loaded)` (which makes both tools reset to empty), then calls `ImportRoads` and `ImportZones` when the loaded data contains them, then reapplies the camera bounds. N (new random world) leaves roads and zones empty, as now. Keep every existing log line unchanged and add one new line after a successful load: `Loaded roads: <segments> segments, <nodes> nodes; zones: <cells> painted cells`.
4. If importing fails after the terrain was already applied, the world stays loaded with empty roads or zones and the error line explains why (document this as a known limitation in PROGRESS.md).
5. Verification per `_RULES.md` (no new unit tests are expected for presentation code; the test total must stay the same). Add this manual F5 checklist, marked "unverified manually": place several roads (chained segments) and paint two zone types; press F6; press N (world, roads and zones are replaced); press F7 (the original terrain, ALL roads with their ribbons and node markers, and the zone overlay return exactly; clicking near an old node snaps to it; placing a new road continues with new ids); close and relaunch the game, press F7 (everything loads); edit the saved JSON to break a road node id and press F7 (one clear error line and the rest of the world still loads); the console shows no errors or warnings, none containing `is_inside_tree`.

## Definition of done
- All verification steps in `_RULES.md` pass with real output.
- `docs/PROGRESS.md` updated by append only; TODO.md left unchanged.

## Stop conditions
- Stop and report when complete and verified.
- If the tool-reset order on `WorldApplied` makes the import impossible without changing `TerrainView`, stop and report the options.

## Commit
`P1-4h: save v2 scene wiring for roads and zones` (final commit; one commit per numbered item before it).
