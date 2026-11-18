# Task 019: Save v3 (buildings, population and simulation state)

> **Read `prompts/_RULES.md` first and follow it. It overrides anything below that conflicts with it.**

## Objective
Save and load the full vertical-slice state: placed buildings, households and citizens, and the simulation state needed to CONTINUE deterministically (tick counter and random stream states). After a load the simulation must continue exactly as the original run would have.

## Read first
1. `AGENTS.md`, then `prompts/_RULES.md`
2. `ARCHITECTURE.md`: save/load section and determinism section
3. `docs/PROGRESS.md`: save v2 (tasks 006 and 017) and the simulation tasks (P2a, P2b)
4. `src/Persistence/`, `src/World/DeterministicRandom.cs`, `src/World/RandomStreams.cs`, `src/Simulation/`, `src/Buildings/`, `src/Population/`

## Allowed changes
- `src/Persistence/`, `src/Simulation/`, `src/Buildings/`, `src/Population/` (export and restore additions only), `src/World/DeterministicRandom.cs` (state export and restore only)
- `src/Presentation/SimulationDriver.cs` and `src/Presentation/Main.cs` for the wiring
- New and extended tests; `docs/PROGRESS.md` (append-only). No `.tscn` edits. Do NOT check off TODO.md items that need the manual test.

## Requirements
1. `DeterministicRandom` gets a way to read its current state and a constructor or factory that restores it, so a restored stream continues with exactly the same sequence (the existing seed-0 reference value and all existing tests stay unchanged).
2. `SimulationSaveData`: tick counter, the state of every named random stream, placed buildings (asset id, parcel, orientation or variant, occupancy), households and citizens with their relations (employment, housing, wealth, happiness). All collections in ascending id order; ids never reused after load (save the next-id counters).
3. `SaveFormat.CurrentFormatVersion` becomes 3. Versions 1 and 2 still load (no simulation state means a fresh start village). Validate every relation on load (a citizen references an existing household, a household references an existing building within capacity, a building references a registered asset, parcels lie inside valid zone cells and do not overlap) and throw `SaveFormatException` otherwise. Use nullable DTO fields so missing fields fail.
4. THE KEY TEST: run the scripted scenario from task 016 for 100 ticks, save, load into a fresh `CitySimulation`, run 100 more ticks, and compare the full snapshot sequence with an uninterrupted 200-tick run of the same scenario. They must be identical. Add a seeded fuzz variant with random scenarios and random save points.
5. Other tests: round trip of an empty simulation, a populated one and a large one (thousands of citizens); v1 and v2 files still load; every invalid relation case throws; serialize twice gives identical strings; unknown future versions are rejected.
6. Wiring: F6 and F7 include the simulation state (`SimulationDriver` exports it and imports it after `Rebind`). Keep all existing log lines. Manual F5 checklist, marked "unverified manually": run the village for a while, press F6, let it keep running, press F7, and the population, building count and tick return to the saved values and then evolve exactly as before; relaunch and load works; broken JSON gives one clear error and an unchanged world.

## Definition of done
- All verification steps in `_RULES.md` pass with real output; test total increased.
- `docs/PROGRESS.md` updated by append only, including the known limitation list (CellSize and HeightScale are still not saved).

## Stop conditions
- Stop and report when complete and verified.
- If an existing domain class cannot be saved without changing its public behavior, stop and report the options.

## Commit
`P2c: save format v3 with buildings, population and simulation state` (final commit; one commit per numbered item before it).
