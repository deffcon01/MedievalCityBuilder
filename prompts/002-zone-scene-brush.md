# Task 002: Zone painting tool (scene brush)

> **Read `prompts/_RULES.md` first and follow it. It overrides anything below that conflicts with it.**

## Objective
Implement the in-game zone painting tool. The player toggles the tool, selects a zone type, and paints or erases zone cells on the terrain with the mouse, using the existing `ZoneGrid` domain code. Painted cells are shown as a tinted overlay hugging the terrain.

## Read first
1. `AGENTS.md`, then `prompts/_RULES.md`
2. `DESIGN.md` section 5 "Zoning" (paint-brush system, starting zone list)
3. `docs/PROGRESS.md`: the P1-4a zoning section and the road tool sections
4. `src/Zoning/ZoneGrid.cs`, `src/Zoning/ZoneRegistry.cs`
5. `src/Presentation/RoadTool.cs`: the pattern to follow (toggle key, picking, ghost, label layer, reset on `WorldApplied`, visuals container with `TopLevel = true`)
6. `src/Presentation/TerrainView.cs`, `src/Presentation/Main.cs`, `src/World/TerrainRaycaster.cs`, `src/World/TerrainSurface.cs`
7. `src/Roads/RoadRibbonBuilder.cs` and `tests/MedievalCityBuilder.Tests/RoadRibbonTests.cs`: the mesh-geometry and winding test pattern

## Allowed changes
- New `src/Zoning/ZoneOverlayBuilder.cs` and `src/Zoning/ZoneOverlayMesh.cs` (pure C#)
- New tests for them
- New `src/Presentation/ZoneTool.cs` and `src/Presentation/ToolMode.cs`
- `src/Presentation/RoadTool.cs`: ONLY the small change needed so the road tool and zone tool are never active together
- `src/Presentation/Main.cs`: minimal wiring (create the tool in code)
- `docs/PROGRESS.md` (append-only). Do NOT edit any `.tscn` file. Do NOT check off TODO.md; the user does that after the F5 test.

## Requirements
1. Pure overlay geometry. `ZoneOverlayBuilder.Build(ZoneGrid grid, TerrainSurface surface, double heightOffset)` returns a `ZoneOverlayMesh` with `float[] Positions` (x, y, z per vertex), `int[] VertexZoneIds` (one zone id per vertex) and `int[] Indices`. Each painted cell (zone not None) becomes one quad: 4 vertices at the cell corners, each corner height sampled with `surface.SampleHeight` at the corner (clamped inside the surface bounds) plus `heightOffset`, and 2 triangles. Cells are emitted in row-major order. Triangle winding must give a NEGATIVE Y component for (b - a) x (c - a) in every triangle. Validate arguments (null, NaN, infinite offset). A cell grid that extends past the terrain must clamp, not throw.
2. Overlay tests: empty grid gives 0 vertices; one cell gives 4 vertices and 2 triangles with the exact corner positions on a flat fixture and on a ramp fixture; every triangle has negative Y winding; several zone ids map to the right `VertexZoneIds`; row-major order; vertex count equals 4 x `PaintedCellCount`; determinism; a grid larger than the terrain clamps without throwing; a non-binary fixture (cell size 0.1 or 0.3); a seeded fuzz test (`DeterministicRandom`) painting random brushes and checking counts against a brute-force count.
3. `ToolMode`: a tiny shared class in `src/Presentation/` that records which editing tool is active (none, road, zone). `RoadTool` (R) and `ZoneTool` (Z) each refuse to turn on while the other is active and log or show a short message such as "Turn off the road tool first (R)". Changes to `RoadTool` stay minimal.
4. `ZoneTool` (partial class, `Node3D`, created in code from `Main.cs`; `Initialize(TerrainView terrain)` logs exactly one line: `ZoneTool: ready (Z toggles zone tool)`):
   - Z toggles the tool. Number keys 1-8 select a zone from the starting registry by registry order. The keys `[` and `]` change the brush radius by 0.5 within 0.5 to 8.0 (PLACEHOLDER values).
   - The grid is created in `Initialize` and again whenever `TerrainView.WorldApplied` fires: `new ZoneGrid(ZoneRegistry.CreateStartingRegistry(), surface.MinX, surface.MinZ, cellSize, cellsX, cellsZ)` with `cellSize` 1.0 (PLACEHOLDER) and the cell counts rounded up to cover the terrain extents. After a world change the grid is empty (zones are saved by tasks 006 and 017).
   - Left mouse button paints, right mouse button erases, both continuously while the button is held and the mouse moves. Picking uses the same raycast as `RoadTool`. All changes go through `ZoneGrid.Paint` and `ZoneGrid.Erase`; no grid logic in the presentation layer.
   - Overlay: one `MeshInstance3D` named `ZoneOverlay` under a `TopLevel` Node3D at the origin. Rebuild it from `ZoneOverlayBuilder` at most once per frame and only when the grid changed. Unshaded material with transparency and vertex colors; color by zone category (placeholder colors: residential green, commercial blue, industrial orange, mixed-use purple), alpha about 0.55. Overlay height offset 0.15.
   - A brush ghost: a semi-transparent flat disc (`CylinderMesh`, height 0.05) with the brush radius at the picked point. Add each child to the tree BEFORE setting its `GlobalPosition`.
   - A label layer in the style of `RoadTool`: tool state, selected zone name, brush radius and painted cell count.
   - Do not consume the mouse wheel. Mark only left and right button events as handled.
5. Verification per `_RULES.md`; the headless output may include the new `ZoneTool: ready (Z toggles zone tool)` line. Add this manual F5 checklist to PROGRESS.md, marked "unverified manually": press Z and see the ghost disc; keys 1-8 change the selected zone and the label; left-drag paints a tinted overlay that hugs hills; right-drag erases; `[` and `]` resize the brush; the overlay is visible from above and while orbiting (no invisible faces); R while the zone tool is on does nothing except the message (and Z while the road tool is on likewise); the wheel zoom and WASD/Q/E still work; N and F7 clear the zones (known limitation); the Godot console shows no warnings or errors.

## Definition of done
- All verification steps in `_RULES.md` pass with real output; test total increased.
- `docs/PROGRESS.md` updated by append only, with the manual checklist marked "unverified manually".
- TODO.md left unchanged.

## Stop conditions
- Stop and report when complete and verified.
- If terrain raycasting cannot support cell-level precision, stop and report the options.

## Commit
`P1-4b: zone painting scene brush` (the final commit; earlier items get their own commits).
