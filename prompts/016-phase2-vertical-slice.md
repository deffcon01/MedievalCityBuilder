# Task 016: Phase 2 vertical slice integration

## Objective

Wire the full loop into a playable vertical slice: generate a test world, view the village, build a road, paint residential zoning, trigger housing demand, generate houses, add households, grow population, paint commercial zoning, generate a commercial building, assign workers, save and load.

## Read first

1. `AGENTS.md` — mandatory workflow
2. `GOAL.md` — "Current implementation priority"
3. `TODO.md` — Phase 2 checklist (the 12 numbered steps)
4. `docs/PROGRESS.md` — all Phase 1 sections
5. All of `src/` — this task wires existing systems together

## Allowed changes

- `src/Simulation/` — orchestration/tick loop
- `src/Presentation/` — wiring, debug output
- `scenes/Main.tscn`, `src/Presentation/Main.cs` — minimal integration
- `src/Persistence/` — extend save with buildings/population if needed (version bump)
- New tests
- `docs/PROGRESS.md`, `TODO.md`

## Requirements

1. A simulation tick that: processes demand events, develops buildings in zoned areas, creates households, places them, grows population over time (placeholder rates), assigns workers.
2. Debug visibility: console/log output showing population, households, buildings, jobs, homeless count each N ticks. No UI required beyond the existing placeholder visuals.
3. Save/load must round-trip the whole slice state (world, roads, zoning, buildings, population).
4. The slice must run headless for verification: a deterministic scripted scenario (seed + fixed ticks) with a headless test asserting the end state.
5. This is the first major milestone — take it in small commits, verify each.

## Definition of done

- `dotnet build` — 0 warnings, 0 errors; `dotnet test` — all pass including the end-to-end scenario test
- `tools/run-headless.ps1` — runs the slice; expected lines documented; no WARNING/ERROR
- Manual F5 checklist in PROGRESS.md (build road, paint zones, watch houses appear)
- `docs/PROGRESS.md` updated; TODO.md Phase 2 items checked

## Stop conditions

- Stop and report when complete and verified.
- If wiring reveals a design conflict between systems, stop and report the conflict with options instead of forcing a change.
- Do not add services, economy, kingdom trade, or events — Phase 3+.

## Commit

Multiple small commits, prefix `P2:` — final one: `P2: first playable vertical slice`
