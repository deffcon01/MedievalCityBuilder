# Task 016: Simulation tick and scripted scenario (domain)

> **Read `prompts/_RULES.md` first and follow it. It overrides anything below that conflicts with it.**

## Objective
Tie the Phase 1 systems together into one deterministic simulation tick, and prove the whole chain with a scripted end-to-end scenario in unit tests: roads and zoning lead to demand events, parcel and asset selection, buildings, households, employment and basic wealth and happiness. This is pure domain code with no scene work (the scene wiring is task 018).

## Read first
1. `AGENTS.md`, then `prompts/_RULES.md`
2. `GOAL.md`, `DESIGN.md` sections 3 (starting village), 6 to 10 (parcels, development events, assets, building selection) and 15 to 16 (population, needs), `ARCHITECTURE.md` (simulation timing, determinism)
3. `TODO.md` Phase 2 "First playable vertical slice"
4. `docs/PROGRESS.md`: every section for tasks 001 to 015
5. The code those tasks created (read the real APIs first, do not assume): `src/Simulation/`, `src/Buildings/`, `src/Population/`, `src/Zoning/`, `src/Roads/`, `src/World/`

## Allowed changes
- New files under `src/Simulation/` (and small additions to existing domain classes only where an item below needs a missing accessor; list every such change in PROGRESS.md)
- New tests; `docs/PROGRESS.md` (append-only); `TODO.md` (domain items of Phase 2 only)
- No `src/Presentation` changes and no scene edits.

## Requirements
1. `CitySimulation` (plain C#): owns references to the world state it needs (terrain surface, water settings, road graph, zone grid and size rules, building registry, placed buildings, households and citizens, demand queue, tick counter) and named random streams from `RandomStreams.Derive(seed, name)`. `Tick()` runs fixed, documented phases in this order and each phase is a separate public or internal method so it can be tested alone: (a) raise demand events, (b) process the demand queue through zone search, parcel fitting, asset selection and building placement, (c) create households and place them in homes, (d) assign employment, (e) update wealth and happiness. Every loop has a bounded amount of work per tick (document the bound).
2. A starting-village builder for tests (`ScriptedScenario`): seed 12345, Small world, a short scripted road network, a painted residential zone and a painted commercial zone near the roads, the starting village from DESIGN.md section 3 reduced to what the domain classes support (a few houses, 8 to 12 citizens), then N ticks.
3. `CitySnapshot` (population, households, buildings by category, jobs, filled jobs, homeless citizens, tick) from `CitySimulation.GetSnapshot()`.
4. Invariants asserted after EVERY tick in the scenario and in a fuzz test: no building exceeds its capacity; every citizen belongs to exactly one household; every household is housed or counted as homeless; job counts are consistent; placed building parcels never overlap each other or leave valid zone cells; ids are unique.
5. Determinism: the same seed and the same scripted inputs give an identical snapshot sequence on two runs. Add a GOLDEN test for the scripted scenario (seed 12345, 200 ticks) that locks the final snapshot values, with a comment that an intentional simulation change requires updating the golden values and noting old and new values in PROGRESS.md.
6. A seeded fuzz test (`DeterministicRandom`): random road and zone setups on small worlds, 300 ticks each, checking the invariants every tick.
7. Record in PROGRESS.md which Phase 2 vertical-slice steps (TODO.md list items 1 to 12) are now covered by domain tests and which still need the scene (task 018) or save support (task 019).

## Definition of done
- All verification steps in `_RULES.md` pass with real output; test total increased.
- `docs/PROGRESS.md` updated by append only.

## Stop conditions
- Stop and report when complete and verified.
- If the existing task 007 to 015 APIs cannot be combined without redesign, or a rule in DESIGN.md conflicts, stop and report the options. Do not invent finalized balance values; every rate and threshold is a configurable PLACEHOLDER.

## Commit
`P2a: simulation tick and scripted scenario` (final commit; one commit per numbered item before it).
