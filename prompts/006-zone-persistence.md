# Task 006: Zone persistence (save/load)

## Objective

Persist the zone grid and registry in the save file, with a save format version increment and migration handling.

## Read first

1. `AGENTS.md` — mandatory workflow
2. `ARCHITECTURE.md` — Save/load section (what must be preserved; versioned save migration)
3. `src/Persistence/WorldSaveData.cs`, `WorldSaveSerializer.cs`, `WorldSaveFile.cs`, `SaveFormatException.cs` — existing save system and DTO pattern
4. `src/Zoning/ZoneGrid.cs`, `ZoneRegistry.cs`

## Allowed changes

- `src/Persistence/` — extend save data/serializer
- `src/Zoning/` — only if a serialization helper is cleaner there
- New tests
- `docs/PROGRESS.md`, `TODO.md`

## Requirements

1. Save: zone registry contents (id, name, category) and the raw zone cell grid, plus grid geometry (minX, minZ, cellSize, cellsX, cellsZ).
2. Increment the save format version. Loading an older save without zone data must either migrate cleanly (empty grid) or fail with a clear SaveFormatException — follow the existing version pattern; document the choice.
3. Round-trip: save -> load must reproduce the exact grid (test with the seeded fuzz-style patterns used in ZoneGridTests).
4. Follow the existing DTO pattern so domain records never leak property names into the wire format.
5. Wire into the existing F6/F7 save/load keys.

## Definition of done

- `dotnet build` — 0 warnings, 0 errors; `dotnet test` — all pass
- `tools/run-headless.ps1` — unchanged, no WARNING/ERROR
- `docs/PROGRESS.md` updated; TODO.md "Zone persistence" checked

## Stop conditions

- Stop and report when complete and verified.
- If the existing save format has no version field, stop and report the migration options before changing the format.

## Commit

`P1-4g: zone persistence with save format version bump`
