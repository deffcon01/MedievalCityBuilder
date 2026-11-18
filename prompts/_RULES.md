# Shared task rules

These rules apply to EVERY prompt in this library. If a prompt conflicts with this file, this file wins. Each rule exists because an earlier run went wrong in that specific way.

## Environment
- Windows, Windows PowerShell 5.1: there is no `&&` between commands; run commands on separate lines.
- Godot 4.7.2 .NET (C#). Game project `MedievalCityBuilder.csproj`; tests in `tests/MedievalCityBuilder.Tests/` (xUnit, .NET 8).
- Verification scripts: `tools/build.ps1`, `tools/run-headless.ps1`. Run tests with `dotnet test tests/MedievalCityBuilder.Tests` directly and read its real output (`tools/test.ps1` only prints a one-line summary).
- The user creates and merges branches. Stay on the current branch. Never switch branches, merge, rebase, force-push, `reset --hard`, `clean`, or delete branches.

## Workflow
- Read AGENTS.md first, then this file, then the prompt. Do not re-plan at length: write the code.
- Edit existing files with one precise str_replace per change; create new files with a single create call. After editing a file, view the changed region and confirm it is intact (no stray blank lines, XML doc comments intact).
- One commit per numbered item in the prompt's Requirements. The prompt's final Commit line is the message for the last commit.
- Hard limits: stop after 40 tool calls, or after 3 failed attempts on the same error, or when the same tool call fails twice in a row (for example a loop of failed read or skill calls). Then write a short status and finish. If the provider drops the stream, stop; the user re-runs.
- Do not enter plan mode and do not wait for further input.
- If a design decision is needed that DESIGN.md and this file do not cover, stop and report the options. Do not guess.

## Code rules
- Domain code is plain C# with explicit `using` directives and no Godot types. Only `src/Presentation/` may use Godot. Every `.cs` file must compile in the Godot project (`tools/build.ps1`), not only in the test project.
- Every new folder under `src/` needs `<Compile Include="..\..\src\<Folder>\**\*.cs" />` in `tests/MedievalCityBuilder.Tests/MedievalCityBuilder.Tests.csproj`. Never add a ProjectReference. No new NuGet packages unless the prompt names them.
- Determinism: no `System.Random`, no `GetHashCode()` for ordering or seeding, no `DateTime`, no `Guid`, no `Math.Pow`, `Math.Sin` or `Math.Cos` in domain code. Use `DeterministicRandom` and `RandomStreams`. Iterate in a defined order (sorted by id, or row-major). Use iterative loops for graph and grid traversals, never recursion.
- Floating point: clamp in double space BEFORE casting to int (casts of huge doubles differ between x64 and ARM). Interpolate as `a * (1 - t) + b * t`, use the endpoints exactly, then clamp to bounds. Cap loop counts that come from a division (tiny spacing values). Use `checked` for money arithmetic, and write integer ceiling division so it cannot overflow.
- Validation: reject NaN and infinity. Throw `ArgumentOutOfRangeException(paramName, message)` with the parameter name FIRST, never a message as the first argument. Missing required fields in saves must fail, never default to 0. Use `CultureInfo.InvariantCulture` for saves.
- Presentation: never edit `.tscn` files (hand-edited scene files caused UID warnings); create nodes in code from `Main.cs`, like `RoadTool` does. Add a child to the tree BEFORE setting `GlobalPosition` or reading `GlobalTransform` (otherwise Godot logs `!is_inside_tree()`). Do not consume mouse-wheel events (the camera zoom uses them). Winding for faces visible from above: for each triangle (a, b, c) the Y component of (b - a) x (c - a) must be negative (Godot front faces are clockwise).
- Do not hard-code content lists in logic (zones, buildings, species): data-driven per AGENTS.md. Values that DESIGN.md says need playtesting are configurable placeholders, labeled PLACEHOLDER in comments.
- Never create `.uid` or `.import` files by hand. Godot creates them when the user opens the editor, and the user commits them.

## Tests
- Every domain feature gets tests: validation of every public argument, boundary values (exactly at a limit, one below, one above), empty inputs, determinism (same input twice gives identical output), and a seeded fuzz test with `DeterministicRandom` compared against a brute-force or reference model.
- Float-math tests use hand-built fixtures with binary-exact values AND at least one non-binary fixture (for example cell size 0.1 or 0.3, odd origins) plus map-edge cases.
- Do not change existing assertions, weaken tests, or delete tests. If a test fails, fix the production code only when the test is correct; never edit expected values to make a test pass. If a golden value must change because an algorithm intentionally changed, record the old and new values in PROGRESS.md.
- Tests must produce no compiler warnings (nullable warnings included).

## Verification (run every command and read the real output)
1. `dotnet build MedievalCityBuilder.csproj`: 0 warnings, 0 errors.
2. `tools/build.ps1`: succeeds.
3. `dotnet test tests/MedievalCityBuilder.Tests`: `failed: 0`, the total is higher than before (unless the prompt adds no tests), and the output contains NO warning lines.
4. `tools/run-headless.ps1`: the expected log lines are unchanged (the two TerrainView lines, `MedievalCityBuilder: Main scene loaded`, the Keys hint, `RoadTool: ready (R toggles road tool)`, plus any new ready line the prompt allows), and no line starts with WARNING or ERROR.
5. The seed 12345 diagnostic line printed during tests must stay `Valid=800, Underwater=99, TooSteep=0, OutOfBounds=0, TooShort=0, Total=899` unless the prompt intentionally changes terrain or road validation.
Report numbers exactly as printed. Never state a test count, a pass, or a clean build that you did not read from real output. A headless run cannot test input or rendering: list a manual F5 checklist marked "unverified manually" and do NOT check off TODO.md items that depend on it. The user checks them off after trying them.

## docs/PROGRESS.md (append-only)
Use ONLY this pattern and never create_file, str_replace or a full rewrite on that file:
`Add-Content -Path docs\PROGRESS.md -Value "<your text>"`
(for multi-line text use a single-quoted here-string). After recording, run `git diff --stat -- docs/PROGRESS.md` and confirm it shows ONLY insertions (zero deletions). If anything was deleted, run `git checkout -- docs/PROGRESS.md` and redo the append. Record: what changed, the verification output (the Test summary line, the diagnostic line, the headless lines), the manual checklist if any, known limitations, and `git log --oneline -8`.

## Final report
Your last message is one short paragraph: what was done, the real verification numbers, and anything not done and why. Then stop.

## Decisions already made (do not revisit)
- Zones below their minimum size are FLAGGED as not developable. Painting is never blocked and nothing is erased automatically.
- Roads and zones are saved together in one save format version bump (v2). Version 1 saves still load, with no roads and no zones. Buildings, population and simulation state come later in v3.
- `RoadGraph` and `ZoneGrid` are separate domain classes. The presentation nodes `RoadTool` and `ZoneTool` own the live instances and recreate them whenever `TerrainView.WorldApplied` fires.
- Only one editing tool is active at a time (R road tool, Z zone tool).
