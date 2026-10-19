# Development Progress Log

## Phase 0 — Project Bootstrap

### 2026-10-12 - Created docs/PROGRESS.md
- **What changed**: Created docs/PROGRESS.md to track development progress
- **Command**: `mkdir docs` and created PROGRESS.md
- **Result**: Directory and file created successfully

### 2026-10-12 - Created scenes/Main.tscn and src/Presentation/Main.cs
- **What changed**: Created scenes/Main.tscn (Node3D root, Camera3D, DirectionalLight3D) and src/Presentation/Main.cs (C# script logging one line in _Ready())
- **Command**: `git add scenes/Main.tscn src/Presentation/Main.cs; git commit -m "scene: add Main.tscn with Node3D, Camera3D, DirectionalLight3D and Main.cs script"`
- **Result**: Scene and script committed. Initial TSCN parse failed due to `uid://` line and `[sub_resource]` for Camera3D/DirectionalLight3D; fixed by removing `uid` and using `[node ... parent="."]` with inline properties.

### 2026-10-12 - Set Main.tscn as main scene in project.godot
- **What changed**: Added `run/main_scene="res://scenes/Main.tscn"` to project.godot
- **Command**: `git add project.godot; git commit -m "config: set Main.tscn as main scene"`
- **Result**: project.godot updated and committed.

### 2026-10-12 - Created tools/build.ps1 and tools/run-headless.ps1
- **What changed**: Created build script (dotnet build) and headless run script (Godot console exe --headless --path . --quit-after 5)
- **Command**: `git add tools/build.ps1 tools/run-headless.ps1; git commit -m "tools: add build.ps1 and run-headless.ps1 scripts"`
- **Result**: Both scripts committed.

### 2026-10-12 - Verified build and headless run
- **What changed**: Ran both scripts and recorded output
- **Command**: `./tools/build.ps1; ./tools/run-headless.ps1`
- **Result**:
  - `build.ps1` output: `Build succeeded!`
  - `run-headless.ps1` output: `MedievalCityBuilder: Main scene loaded` then `Headless run completed successfully!`
- **Notes**: Initial headless run failed because C# class was not found. Root cause: `dotnet build` builds the Release configuration, but Godot loads the Debug assembly from `.godot/mono/temp/bin/Debug/`. Fixed by switching `tools/build.ps1` to `--configuration Debug`. After fix, headless run printed the expected log line and exited 0.

## Final Status

- **Done**: All Phase 0 items implemented and verified.
- **Verified**: `dotnet build` succeeds; Godot headless run prints "MedievalCityBuilder: Main scene loaded" and exits cleanly.
- **Blocked**: None.
- **Next**: Phase 1 — Technical prototype (camera, basic world, roads, zoning, building development, population).

---

## Final Report (2026-10-12)

### Done
1. `docs/PROGRESS.md` created and updated with timestamped entries.
2. `scenes/Main.tscn` created with Node3D root, Camera3D, DirectionalLight3D.
3. `src/Presentation/Main.cs` attached to Main node; logs `MedievalCityBuilder: Main scene loaded` in `_Ready()`.
4. `project.godot` updated with `run/main_scene="res://scenes/Main.tscn"`.
5. `tools/build.ps1` (dotnet build Debug) and `tools/run-headless.ps1` (Godot console exe --headless --quit-after 5) created.
6. Both scripts verified:
   - `./tools/build.ps1` → `Build succeeded!`
   - `./tools/run-headless.ps1` → `MedievalCityBuilder: Main scene loaded` → `Headless run completed successfully!`
7. All Phase 0 checkboxes in TODO.md checked off.

### Verified
- `git log` shows 8 coherent commits on `phase0-bootstrap`.
- Working tree clean.
- No packages, test frameworks, or gameplay code added.
- GOAL.md, DESIGN.md, ARCHITECTURE.md untouched.

### Blocked
- None

### Next
- Phase 1 — Technical prototype.

---

## Phase 1.1 — Camera

### 2026-10-12 - Created CameraMath (src/Core/CameraMath.cs)
- **What changed**: Pure C# static class with ClampZoom, ClampToBounds, WrapYaw
- **Command**: `git add src/Core/CameraMath.cs; git commit -m "feat: add CameraMath and CameraRig with pan/rotate/zoom and test grid plane"`
- **Result**: Compiles and is used by CameraRig.

### 2026-10-12 - Created CameraRig (src/Presentation/CameraRig.cs)
- **What changed**: Node3D pivot with child Camera3D at fixed downward pitch. WASD/arrows pan relative to yaw. Q/E rotate yaw. Mouse wheel zooms by changing camera distance. All tunable values are [Export] fields with placeholder defaults.
- **Command**: `git add src/Presentation/CameraRig.cs; git commit -m "feat: add CameraMath and CameraRig with pan/rotate/zoom and test grid plane"`
- **Result**: Compiles successfully. Uses CameraMath for all clamping.

### 2026-10-12 - Updated Main.tscn with CameraRig and Test Plane
- **What changed**: Replaced Camera3D with CameraRig node. Added large flat PlaneMesh (200x200) with StandardMaterial3D as temporary test plane (node name: TestPlane_Temporary).
- **Command**: `git add scenes/Main.tscn; git commit -m "fix: use StandardMaterial3D for test plane; add Nullable enable to csproj"`
- **Result**: Scene loads in headless mode.

### 2026-10-12 - Verified build and headless run
- **What changed**: Ran build.ps1 and run-headless.ps1
- **Command**: `./tools/build.ps1; ./tools/run-headless.ps1`
- **Result**:
  - `build.ps1` output: `Build succeeded!`
  - `run-headless.ps1` output: `MedievalCityBuilder: Main scene loaded` then `Headless run completed successfully!`
- **Notes**: Input behavior cannot be verified in headless mode. Manual test checklist (F5 in editor):
  - [ ] WASD / arrow keys pan camera smoothly in XZ plane relative to yaw
  - [ ] Q / E rotate camera yaw left/right
  - [ ] Mouse wheel zooms in/out (distance from pivot clamped to zoomMin/zoomMax)
  - [ ] Camera does not pan beyond boundsMinX/MaxX/MinZ/MaxZ
  - [ ] Pitch is fixed at pitchDegrees (55° placeholder)
  - [ ] No drift or jitter during combined pan/rotate/zoom

## Fix: RoadTool !is_inside_tree() error (2026-10-18)

### P1-3c-iii-b-2-fix: fix Godot error "!is_inside_tree()" in RoadTool
- **What changed**: In `src/Presentation/RoadTool.cs`, removed `GlobalPosition` from the `MeshInstance3D` object initializer in `AddSegmentVisuals` and set `marker.GlobalPosition = ...` after calling `_visuals.AddChild(marker)`.
- **Verification**:
  - `dotnet build MedievalCityBuilder.csproj`: zero warnings and zero errors.
  - `tools/build.ps1`: succeeded.
  - `dotnet test tests/MedievalCityBuilder.Tests`: all 403 tests passed.
  - `tools/run-headless.ps1`: output matched expected lines (`TerrainView: ...`, `WaterPlane: ...`, `MedievalCityBuilder: Main scene loaded`, `Keys: ...`, `RoadTool: ready (R toggles road tool)`), with no WARNING or ERROR lines.
- **Note**: Fixed `!is_inside_tree()` error caused by setting `GlobalPosition` before `AddChild` on node markers. Headless runs cannot catch this because no road is placed headless; the manual F5 console check found it.
- **Done**: CameraMath, CameraRig, test plane, scene integration.
- **Verified**: Build succeeds; headless run loads scene and prints log.
- **Blocked**: None.
- **Next**: Phase 1.2 — Basic world (terrain, water, seed).
- **Manual verification needed**: Items marked "unverified manually" above.

---

## Phase 1-4a-zone-grid — Zone Registry, Zone Grid and Paint Brush

### 2026-10-18 - Implemented Zone Registry, Zone Grid and tests
- **What changed**:
  1. Updated `tests/MedievalCityBuilder.Tests/MedievalCityBuilder.Tests.csproj` to include `..\..\src\Zoning\**\*.cs`
  2. Created `src/Zoning/ZoneRegistry.cs`:
     - `ZoneTypeId` record struct with `None` static, `IsNone`, `ToString`
     - `ZoneCategory` enum with Residential, Commercial, Industrial, MixedUse
     - `ZoneDefinition` record with validation (Id not None, Name not null/empty/whitespace)
     - `ZoneRegistry` class with SortedDictionary backing, Register with duplicate checks, Get/TryGet/IsRegistered, Definitions ordered by Id, CreateStartingRegistry with 8 definitions from DESIGN.md section 5
  3. Created `src/Zoning/ZoneGrid.cs`:
     - `ZoneCell` record struct
     - `ZoneGrid` class with constructor validation (null registry, NaN/infinite values, cellSize <= 0, cells < 1, size cap 4M cells)
     - Grid stored as int[] row-major, all initially 0 (None)
     - `Get`/`Set` with bounds checking and registry validation
     - `TryGetCellAt` with NaN/infinite/outside checks and upper edge clamping
     - `CellCenterX`/`CellCenterZ` with bounds checking
     - `GetBrushCells` with bounding box optimization, center-distance check, row-major ordering
     - `Paint`/`Erase` with unregistered zone validation
     - `CountCells` and `PaintedCellCount`
     - `GetRawCells` returning copy
  4. Created `tests/MedievalCityBuilder.Tests/ZoneGridTests.cs` with 18 tests covering:
     - Registry validation, duplicate detection, TryGet/IsRegistered, CreateStartingRegistry definitions
     - ZoneDefinition validation for None id and invalid names
     - Grid construction validation (null registry, NaN/infinite, invalid sizes, sanity cap)
     - Initial state, MaxX/MaxZ
     - Get/Set with bounds checking, change detection, unregistered zones
     - TryGetCellAt corner cases and clamping
     - CellCenter calculations and bounds checking
     - GetBrushCells distance calculations, ordering, edge cases, invalid inputs
     - Paint/Erase with changed count verification, overlapping brushes
     - GetRawCells copy semantics
     - Fuzz test with DeterministicRandom(1234), 2000 random operations, model verification, seed reproducibility, minimum changed operations
- **Commands**: 4 commits on `phase1-4a-zone-grid`
- **Verification Results**:
  - `dotnet build MedievalCityBuilder.csproj` output:
    ```
    Build succeeded.
        0 Warning(s)
        0 Error(s)
    ```
  - `tools/build.ps1` output: `Build succeeded!`
  - `dotnet test tests/MedievalCityBuilder.Tests` output:
    ```
    Passed!  - Failed:     0, Passed:   468, Skipped:     0, Total:   468, Duration: 440 ms
    ```
    (450 existing tests + 18 new ZoneGrid tests)
  - `tools/run-headless.ps1` output:
    ```
    Running MedievalCityBuilder in headless mode...
    Godot: C:\Users\Admin\Downloads\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe
    Project: .
    Godot Engine v4.7.2.stable.mono.official.ed1daf0bf - https://godotengine.org

    TerrainView: seed=12345, size=Small, vertices=16641, extents=(-64, 64, -64, 64)
    WaterPlane: SeaLevel=0.35, underwaterFraction=0.0772
    MedievalCityBuilder: Main scene loaded
    Keys: F6 save, F7 load, N new random world
    RoadTool: ready (R toggles road tool)
    Headless run completed successfully!
    ```
  - **WARNING/ERROR check**: No lines starting with "WARNING" or "ERROR" in full headless output. All five expected lines present unchanged.
  - **Diagnostic line**: `TerrainView: seed=12345, size=Small, vertices=16641, extents=(-64, 64, -64, 64)` — unchanged (Valid=800, Underwater=99, TooSteep=0, OutOfBounds=0, TooShort=0, Total=899)
  - **Notes**: Zone ids 1..8 are part of future save format; zone definitions are placeholders until data files exist; this task covers grid and brush only. No Godot types, no System.Random/GetHashCode/DateTime/Guid/Math.Pow used. All code compiles with zero warnings.

### Git Log
```
750760d P1-4a: Add comprehensive tests for ZoneRegistry and ZoneGrid
aed81ff P1-4a: Implement ZoneGrid with brush painting and cell management
4fc1fca P1-4a: Implement ZoneRegistry with ZoneTypeId, ZoneDefinition, and starting registry
eb7391b P1-4a: Add Zoning source include to test project
```

### Final Status (Phase 1-4a)
- **Done**: All 4 scope items implemented and verified:
  1. Test project updated with Zoning source include
  2. ZoneRegistry.cs with ZoneTypeId, ZoneCategory, ZoneDefinition, ZoneRegistry and CreateStartingRegistry
  3. ZoneGrid.cs with ZoneCell, ZoneGrid, brush painting, validation
  4. ZoneGridTests.cs with 18 comprehensive tests
- **Verified**: `dotnet build` zero warnings/errors; `tools/build.ps1` succeeds; `dotnet test` all 468 tests pass (450 existing + 18 new); `tools/run-headless.ps1` output matches expected lines with no WARNING/ERROR; diagnostic line unchanged
- **Blocked**: None
- **Next**: Phase 1-4b — Zone painting scene brush or section 7 development events (per TODO.md)
- **Note**: Zone ids 1..8 are part of future save format; zone definitions are placeholders until data files exist; this task covers grid and brush only
