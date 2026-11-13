# Task 015: Employment, wealth, happiness

## Objective

Implement basic employment, wealth, and happiness — completing Phase 1.6.

## Read first

1. `AGENTS.md` — mandatory workflow
2. `DESIGN.md` — Section 5 (worker counts per parcel/zone type), Section 15, Section 16 (needs reduce happiness)
3. `src/Population/` — all models and housing logic (tasks 013-014)
4. `src/Buildings/` — building entities (jobs from commercial/industrial buildings)
5. `src/Simulation/` — demand events

## Allowed changes

- `src/Population/` and/or `src/Simulation/` — pure C#
- New tests
- `docs/PROGRESS.md`, `TODO.md`

## Requirements

1. Employment: jobs from commercial/industrial/mixed-use buildings (worker counts from DESIGN.md Section 5, as configurable placeholders); assign citizens to jobs; unemployment = citizens able to work without a job.
2. Wealth: household wealth as a number with placeholder accumulation rules (employed households gain, unemployed lose, small amounts). Keep it simple and configurable.
3. Happiness: base value reduced by unmet needs (housing/homelessness, employment, food placeholder). Placeholder weights, configurable. No UI.
4. All values are placeholders per GOAL.md — do not treat as final balance.
5. Tests: job creation from buildings, assignment, unemployment count, wealth change, happiness calculation, determinism.

## Definition of done

- `dotnet build` — 0 warnings, 0 errors; `dotnet test` — all pass
- `tools/run-headless.ps1` — unchanged, no WARNING/ERROR
- `docs/PROGRESS.md` updated; TODO.md "Basic employment", "Basic wealth", "Basic happiness" checked — Phase 1.6 complete

## Stop conditions

- Stop and report when complete and verified.
- Do not implement services, needs production, or economy — Phase 3+.

## Commit

`P1-6c: employment, wealth, happiness — Phase 1.6 complete`
