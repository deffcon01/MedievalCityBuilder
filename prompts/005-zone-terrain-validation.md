# Task 005: Zone terrain validation

> **Read `prompts/_RULES.md` first and follow it. It overrides anything below that conflicts with it.**

## Objective

Implement terrain suitability validation for zoning: zone cells on unsuitable terrain (underwater, too steep) are flagged as undevelopable.

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
0. Flag only: painting is never blocked and nothing is erased (decision already made in `prompts/_RULES.md`). Unsuitable cells are reported as not developable.

1. Given a world position/cell, return terrain suitability (underwater / too steep / valid) using existing terrain data. Do not duplicate terrain logic — consume existing classes.
2. Slope threshold is a configurable placeholder.
3. Integrate with the validation pipeline from tasks 003/004 so a single query can report: undersized, too far from road, underwater, too steep, valid.
4. Tests: underwater cell, steep cell, valid cell, boundary cases, reproducibility.

## Definition of done

- Every verification step in `prompts/_RULES.md` passes with real output: 0 warnings in `dotnet build MedievalCityBuilder.csproj` AND no warning lines in the `dotnet test` output; the test total increased; `docs/PROGRESS.md` updated by append only
- `tools/run-headless.ps1` — unchanged, no WARNING/ERROR
- `docs/PROGRESS.md` updated; TODO.md "Terrain validation" checked

## Stop conditions

- Stop and report when complete and verified.
- If terrain classes cannot answer slope queries without new code, add the minimal query to src/World rather than duplicating data.

## Commit

`P1-4f: zone terrain validation`
