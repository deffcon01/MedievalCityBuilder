# Task 005: Zone terrain validation

## Objective

Implement terrain suitability validation for zoning: zone cells on unsuitable terrain (underwater, too steep) cannot be painted or are flagged as undevelopable.

## Read first

1. `AGENTS.md` — mandatory workflow
2. `DESIGN.md` — Section 5 "Zoning": terrain suitability requirements; Section 2 Terrain
3. `src/World/TerrainSurface.cs`, `src/World/Heightmap.cs` — terrain queries
4. `src/World/WaterAnalysis.cs` — underwater analysis
5. `src/Zoning/ZoneGrid.cs` and the validation code from tasks 003/004

## Allowed changes

- `src/Zoning/` — terrain validation, pure C#
- New tests
- `docs/PROGRESS.md`, `TODO.md`

## Requirements

1. Given a world position/cell, return terrain suitability (underwater / too steep / valid) using existing terrain data. Do not duplicate terrain logic — consume existing classes.
2. Slope threshold is a configurable placeholder.
3. Integrate with the validation pipeline from tasks 003/004 so a single query can report: undersized, too far from road, underwater, too steep, valid.
4. Tests: underwater cell, steep cell, valid cell, boundary cases, reproducibility.

## Definition of done

- `dotnet build` — 0 warnings, 0 errors; `dotnet test` — all pass
- `tools/run-headless.ps1` — unchanged, no WARNING/ERROR
- `docs/PROGRESS.md` updated; TODO.md "Terrain validation" checked

## Stop conditions

- Stop and report when complete and verified.
- If terrain classes cannot answer slope queries without new code, add the minimal query to src/World rather than duplicating data.

## Commit

`P1-4f: zone terrain validation`
