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

### Final Status (Phase 1.1b)
- **Done**: CameraRig fixes, grid shader, markers, light/camera fixes.
- **Verified**: Build succeeds; headless run loads scene and prints log.
- **Blocked**: None.
- **Next**: Phase 1.2 — Basic world (terrain, water, seed).