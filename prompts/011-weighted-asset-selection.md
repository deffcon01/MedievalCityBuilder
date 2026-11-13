# Task 011: Weighted asset selection

## Objective

Implement weighted building asset selection: pick a building asset for a parcel using weighted randomness, architectural generation/legacy weighting, and anti-repetition.

## Read first

1. `AGENTS.md` — mandatory workflow
2. `DESIGN.md` — Section 8 (Active/legacy/retired states, selection weight, discourage excessive repetition, neighborhood continuity), Section 9 (residential selection: zone + space + demand + household -> pool -> weighted selection)
3. `src/World/DeterministicRandom.cs`, `RandomStreams.cs` — seeded randomness
4. `src/Buildings/` — asset definitions from task 010
5. `src/Simulation/` — demand events, parcels (tasks 007-009)

## Allowed changes

- `src/Buildings/` or `src/Simulation/` — selection code, pure C#
- New tests
- `docs/PROGRESS.md`, `TODO.md`

## Requirements

1. Given a parcel + demand context, filter the eligible pool (zone type, footprint class fits, suitability, unlock state), then weighted-select using a DeterministicRandom stream.
2. Architectural generation weighting: current generation assets normal weight, legacy lower weight, retired excluded (but existing buildings unaffected — this task only affects new selection).
3. Anti-repetition: recent selections of the same asset reduce its near-term weight. Simple decay or blocklist is fine — document the approach.
4. Same seed + same state = same selection. Tests must prove this.
5. Tests: pool filtering, weights respected statistically (bounded sample with fixed seed), legacy weighting, anti-repetition, determinism.

## Definition of done

- `dotnet build` — 0 warnings, 0 errors; `dotnet test` — all pass
- `tools/run-headless.ps1` — unchanged, no WARNING/ERROR
- `docs/PROGRESS.md` updated; TODO.md "Weighted asset selection" and "Architectural generation/legacy weighting" checked

## Stop conditions

- Stop and report when complete and verified.
- Do not implement building placement or visuals.

## Commit

`P1-5e: weighted asset selection with generation and anti-repetition weighting`
