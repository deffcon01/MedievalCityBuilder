# Task 010: Data-driven building asset definitions

> **Read `prompts/_RULES.md` first and follow it. It overrides anything below that conflicts with it.**

## Objective

Implement the data-driven building asset definition system: building assets described as data (zone type, footprint class, capacity, architectural generation, suitability, unlock condition, selection weight) rather than code.

## Read first

1. `AGENTS.md` — mandatory workflow, data-driven rule
2. `DESIGN.md` — Section 8 "Building asset system" (metadata list, asset families, architectural progression states)
3. `ARCHITECTURE.md` — data-driven content section
4. `src/Zoning/ZoneRegistry.cs` — the registry pattern to mirror

## Allowed changes

- New `src/Buildings/` folder — definitions and registry, pure C#
- `data/` — seed data file(s) (JSON or similar) with a small placeholder asset set
- `tests/MedievalCityBuilder.Tests/MedievalCityBuilder.Tests.csproj` — include new source folder
- New tests
- `docs/PROGRESS.md`, `TODO.md`

## Requirements

1. BuildingAssetDefinition record with the metadata fields from DESIGN.md Section 8: zone type, footprint class, capacity, architectural generation, climate/terrain/wealth suitability, unlock condition, visual family, selection weight.
2. A registry that loads definitions from data files at data/ — new assets must be addable without changing core code.
3. Include a small placeholder set (a few residential + commercial + industrial assets) clearly marked as placeholders.
4. Validation: duplicate ids, invalid weights, unknown zone types must fail with clear errors.
5. Tests: load from data, validation failures, ordering, empty registry behavior.

6. Parsing is pure C#: build the registry from a JSON string with `System.Text.Json` (strict validation, culture-invariant, missing required fields fail). Reading the file from `data/` is a thin separate step. If Godot generates `.import` files for the data files, report it in PROGRESS.md so the user can commit them (do not create them by hand).

## Definition of done

- Every verification step in `prompts/_RULES.md` passes with real output: 0 warnings in `dotnet build MedievalCityBuilder.csproj` AND no warning lines in the `dotnet test` output; the test total increased; `docs/PROGRESS.md` updated by append only
- `tools/run-headless.ps1` — unchanged, no WARNING/ERROR
- `docs/PROGRESS.md` updated; TODO.md "Data-driven building asset definitions" checked

## Stop conditions

- Stop and report when complete and verified.
- Do not implement weighted selection or placement.

## Commit

`P1-5d: data-driven building asset definitions`
