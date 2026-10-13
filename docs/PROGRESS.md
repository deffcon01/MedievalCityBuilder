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
- None.

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

### Final Status (Phase 1.1)
- **Done**: CameraMath, CameraRig, test plane, scene integration.
- **Verified**: Build succeeds; headless run loads scene and prints log.
- **Blocked**: None.
- **Next**: Phase 1.2 — Basic world (terrain, water, seed).
- **Manual verification needed**: Items marked "unverified manually" above.

---

## Phase 1.1b — Camera Fixes & Visuals

### 2026-10-12 - Fixed CameraRig.ApplyZoom and _Input
- **What changed**: 
  - Changed `z = -_currentZoom * cos(pitch)` to `z = +_currentZoom * cos(pitch)` so camera sits at +Z and looks toward -Z.
  - Added `mb.Pressed` check in `_Input` for wheel events.
  - Updated comments to match.
- **Command**: `git add src/Presentation/CameraRig.cs; git commit -m "fix: CameraRig.ApplyZoom z sign, add mb.Pressed check for wheel"`
- **Result**: Compiles successfully.

### 2026-10-12 - Added Grid Shader, Markers, Fixed Light & Camera
- **What changed**:
  - Created `assets/shaders/test_grid.gdshader`: unshaded spatial shader drawing grid lines every 1 world unit, thicker every 10 units, using world-space position.
  - Updated `scenes/Main.tscn`: replaced plain material with ShaderMaterial using the grid shader.
  - Added `Marker_North_Temporary` (red 2x2x2 BoxMesh at (0,1,-20)) and `Marker_East_Temporary` (blue 2x2x2 BoxMesh at (20,1,0)) with unshaded StandardMaterial3D.
  - Fixed `DirectionalLight3D` rotation to `rotation_degrees = Vector3(-50, 45, 0)` (downward).
  - Fixed `Camera3D` under `CameraRig` to have `current = true`.
- **Command**: `git add scenes/Main.tscn assets/shaders/test_grid.gdshader; git commit -m "feat: add grid shader material, north/east markers, fix light rotation and camera current"`
- **Result**: Scene loads in headless mode (minor Godot 3.x remap warnings for `render_unshaded`, harmless).

### 2026-10-12 - Verified build and headless run
- **What changed**: Ran build.ps1 and run-headless.ps1
- **Command**: `./tools/build.ps1; ./tools/run-headless.ps1`
- **Result**:
  - `build.ps1` output: `Build succeeded!`
  - `run-headless.ps1` output: `MedievalCityBuilder: Main scene loaded` then `Headless run completed successfully!`
- **Notes**: Input behavior cannot be verified in headless mode. **Manual test checklist (F5 in editor):**
  - [ ] At start: red box (north) is straight ahead, blue box (east) is on the right
  - [ ] W moves camera toward the red box (north/-Z)
  - [ ] D moves camera toward the blue box (east/+X)
  - [ ] After pressing E or Q (rotate yaw), W still moves toward where the camera faces
  - [ ] Mouse wheel zooms one step per notch (distance from pivot clamped to zoomMin/zoomMax)
  - [ ] Panning stops at bounds (camera does not exceed boundsMinX/MaxX/MinZ/MaxZ)
  - [ ] Pitch is fixed at pitchDegrees (55° placeholder)
  - [ ] No drift or jitter during combined pan/rotate/zoom
- **All items marked "unverified manually"**

---

## Phase 1.1c — Camera Zoom Spin Fix & Godot 4 Warnings

### 2026-10-12 - Fixed CameraRig.ApplyZoom
- **What changed**: Removed `_camera.LookAt(Vector3.Zero, Vector3.Up)`. Camera local rotation is now set directly: `_camera.Rotation = new Vector3(-pitchRad, 0f, 0f)`. Added comment explaining LookAt uses global coordinates, which caused zoom to spin the camera once the rig moved away from the origin.
- **Command**: `git add src/Presentation/CameraRig.cs; git commit -m "fix: CameraRig.ApplyZoom sets local rotation instead of LookAt (global coords caused zoom spin)"`
- **Result**: Compiles successfully.

### 2026-10-12 - Fixed Godot 4 material warnings in Main.tscn
- **What changed**:
  - Replaced `render_unshaded = true` (Godot 3.x) with `shading_mode = 0` (Godot 4 StandardMaterial3D) on RedMaterial and BlueMaterial.
  - Corrected the CameraRig ext_resource uid from `uid://bjstp7uvpjwd4` to `uid://cl5oi883y66bg` to match `src/Presentation/CameraRig.cs.uid`. Main.cs uid (`uid://b3evb74rp8xmb`) already matched.
- **Command**: `git add scenes/Main.tscn; git commit -m "fix: Godot 4 StandardMaterial3D shading_mode=0 and correct CameraRig uid in Main.tscn"`
- **Result**: Scene loads in headless mode with no remap warnings.

### 2026-10-12 - Verified build and headless run
- **What changed**: Ran build.ps1 and run-headless.ps1.
- **Command**: `./tools/build.ps1; ./tools/run-headless.ps1`
- **Result**:
  - `build.ps1` output: `Build succeeded!`
  - `run-headless.ps1` full output:
    ```
    Godot Engine v4.7.2.stable.mono.official.ed1daf0bf - https://godotengine.org

    MedievalCityBuilder: Main scene loaded
    ```
  - Exit code 0.
- **WARNING/ERROR check**: No lines starting with "WARNING" or "ERROR" in the full headless output. No unresolved warnings.

### Manual Checklist (unverified manually)
- [ ] After panning away from the start and rotating with Q/E, scrolling the wheel changes only the distance and does not change the view direction or yaw.

### Final Status (Phase 1.1c)
- **Done**: All four items implemented and committed (3 commits on `phase1-camera`).
- **Verified**: `./tools/build.ps1` → `Build succeeded!`; `./tools/run-headless.ps1` → full output recorded above, exit 0, zero WARNING/ERROR lines.
- **Blocked**: None.
- **Next**: Phase 1.2 — Basic world (terrain, water, seed).
- **Remaining**: Manual zoom-after-pan test (see checklist above) still needs F5 in the editor.

### Final Status (Phase 1.1b)
- **Done**: CameraRig fixes, grid shader, markers, light/camera fixes.
- **Verified**: Build succeeds; headless run loads scene and prints log.
- **Blocked**: None.
- **Next**: Phase 1.2 — Basic world (terrain, water, seed).
## Manual verification (user, F5)
Phase 1.1 camera: pan, rotate, zoom, orientation markers and startup console verified manually on 2026-10-12. Result: pass.

---

## Phase 1-Tests — xUnit Test Project Setup

### 2026-10-12 - Added xUnit test project and CameraMath tests
- **What changed**: 
  - Added `<Compile Remove="tests/**" />` to MedievalCityBuilder.csproj so Godot ignores test files
  - Created `tests/MedievalCityBuilder.Tests` xUnit project targeting net8.0
  - Added test project to solution
  - Created `CameraMathTests.cs` with 14 tests covering ClampZoom, ClampToBounds, WrapYaw
  - Created `tools/test.ps1` to run dotnet test
- **Commands**: 
  - `dotnet new xunit -o tests/MedievalCityBuilder.Tests`
  - `dotnet sln MedievalCityBuilder.sln add tests/MedievalCityBuilder.Tests/MedievalCityBuilder.Tests.csproj`
  - `git add ...` (5 commits)
- **Verification Results**:
  - `tools/build.ps1` output:
    ```
    Building MedievalCityBuilder...
    Build succeeded!
    ```
  - `tools/test.ps1` output:
    ```
    Running tests for MedievalCityBuilder...
    All tests passed!
    ```
  - `tools/run-headless.ps1` output:
    ```
    Running MedievalCityBuilder in headless mode...
    Godot: C:\Users\Admin\Downloads\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe
    Project: .
    Godot Engine v4.7.2.stable.mono.official.ed1daf0bf - https://godotengine.org

    MedievalCityBuilder: Main scene loaded
    Headless run completed successfully!
    ```
- **Notes**: All three verification commands pass. CameraMath tests cover all specified cases: ClampZoom (below min, above max, inside range), ClampToBounds (inside, each edge, outside on both axes), WrapYaw (0, 180 → 180, -180 → 180, 190 → -170, -190 → 170, 360 → 0, 540 → 180).

### Final Status (Phase 1-Tests)
- **Done**: xUnit test project created, CameraMath tests implemented, test runner script added, all verifications pass.
- **Verified**: `tools/build.ps1` → Build succeeded; `tools/test.ps1` → All 14 tests passed; `tools/run-headless.ps1` → Scene loads and exits cleanly.
- **Blocked**: None.
- **Next**: Phase 1.2 — Basic world (terrain, water, seed).

---

## Final Report (2026-10-12)

### Done
1. Added `<Compile Remove="tests/**" />` to MedievalCityBuilder.csproj
2. Created `tests/MedievalCityBuilder.Tests` with xUnit, targeting net8.0 (approved dependencies only)
3. Added test project to solution with `dotnet sln add`
4. Created `CameraMathTests.cs` with 14 tests covering all specified cases
5. Created `tools/test.ps1` test runner script
6. Verified all three scripts pass: build, test, headless run
7. Updated PROGRESS.md with full verification output

### Verified
- `tools/build.ps1` builds successfully with zero errors
- `tools/test.ps1` runs 14 tests, all pass
- `tools/run-headless.ps1` loads scene and exits cleanly with exit code 0
- No WARNING/ERROR lines in headless output
- Test project uses only approved dependencies: Microsoft.NET.Test.Sdk, xunit, xunit.runner.visualstudio
- No ProjectReference to Godot project added
- Source files compiled via `<Compile Include>` in test project

### Blocked
- None

### Next
- Phase 1.2 — Basic world (terrain, water, seed)
