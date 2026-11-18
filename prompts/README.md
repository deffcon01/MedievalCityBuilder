# Prompt Library

Bounded, self-contained task prompts for the AI coder (OpenCode). Each file is one vertical slice with explicit scope, verification and stop conditions, following the AGENTS.md workflow. `_RULES.md` holds the rules shared by every prompt; read it first.

## How to use (manual flow, one task at a time)
1. Make sure `main` is up to date and clean:
   ```
   git switch main
   git pull origin main
   git status --short
   ```
2. Create the task branch (use the file name, for example `task/002-zone-scene-brush`):
   ```
   git switch -c task/<NNN-name>
   ```
3. Start `opencode2` in the repo root, choose Build mode, and send:
   `Read prompts/_RULES.md, then prompts/<file> and execute the task exactly. Follow AGENTS.md.`
4. When it finishes, run the verification commands yourself (they are listed in `_RULES.md`) and have the results reviewed before merging.
5. If the task added C# files, open the project in the Godot editor once, close it, and commit the generated `.uid` files.
6. Merge:
   ```
   git switch main
   git merge --no-ff task/<NNN-name>
   git push origin main
   ```

## Rules
- Execute prompts in the ORDER column below, not in file-number order. Each builds on the previous one's output.
- One prompt = one branch = one merge. Never run two prompts in parallel.
- If a prompt stops on a design question, answer it, then re-run the same prompt; do not skip ahead.
- New prompts: copy the template structure (Objective / Read first / Allowed changes / Requirements / Definition of done / Stop conditions / Commit), number the next file, and keep it to one bounded slice.

## Current sequence
| Order | File | Area | Status |
|---|---|---|---|
| 1 | 001-road-entrance-snapping | 1.3 leftover | done (merged) |
| 2 | 002-zone-scene-brush | 1.4 zone painting tool | |
| 3 | 003-zone-minimum-size | 1.4 | |
| 4 | 004-zone-road-distance | 1.4 | |
| 5 | 005-zone-terrain-validation | 1.4 | |
| 6 | 006-zone-persistence | 1.4 save format v2, domain layer (roads and zones) | |
| 7 | 017-save-v2-scene-wiring | 1.4 save v2 in the scene | |
| 8 | 007-development-demand-events | 1.5 | |
| 9 | 008-available-zone-search | 1.5 | |
| 10 | 009-parcel-fitting | 1.5 | |
| 11 | 010-building-asset-definitions | 1.5 | |
| 12 | 011-weighted-asset-selection | 1.5 | |
| 13 | 012-building-placement | 1.5 | |
| 14 | 013-citizen-household-models | 1.6 | |
| 15 | 014-housing-capacity-homelessness | 1.6 | |
| 16 | 015-employment-wealth-happiness | 1.6 | |
| 17 | 016-phase2-vertical-slice | Phase 2: simulation tick and scripted scenario (domain) | |
| 18 | 018-slice-scene-wiring | Phase 2: simulation in the scene | |
| 19 | 019-save-v3-simulation | Phase 2: save v3 (buildings, population, simulation state) | |

Prompts 020+ (Phase 3 and later: services, needs, economy, procedural world) are added after the vertical slice is verified.

## The driver script
`tools/run-prompts.ps1` is NOT recommended yet. Known problems: it is written for the `opencode` command (this machine uses `opencode2`), it checks only the `dotnet test` exit code (not warnings, the headless output or the docs diff), and under Windows PowerShell 5.1 the combination of `$ErrorActionPreference = "Stop"` with `2>&1` on native commands is likely to abort on the first stderr line. Use the manual flow above until the script is fixed and tried once attended.
