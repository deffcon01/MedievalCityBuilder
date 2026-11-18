# Task 003: Zone minimum size (flagging)

> **Read `prompts/_RULES.md` first and follow it. It overrides anything below that conflicts with it.**

## Objective
Implement minimum-size analysis for painted zones. Connected painted regions of one zone type that are smaller than that zone's minimum size are FLAGGED as not developable. Painting is never blocked and nothing is erased (decision already made, see `_RULES.md`). This is a pure domain task: no Godot code.

## Read first
1. `AGENTS.md`, then `prompts/_RULES.md`
2. `DESIGN.md` section 5 "Zoning" ("Each zone has minimum sizing rules, maximum distance from a valid path/road, terrain suitability requirements") and section 6 (parcel generation)
3. `ARCHITECTURE.md`: data-driven rule and testing section
4. `docs/PROGRESS.md`: the P1-4a zoning section
5. `src/Zoning/ZoneGrid.cs`, `src/Zoning/ZoneRegistry.cs` and `tests/MedievalCityBuilder.Tests/ZoneGridTests.cs` (existing test style, including the seeded fuzz test)

## Allowed changes
- New `src/Zoning/ZoneSizeRules.cs`, `src/Zoning/ZoneRegion.cs`, `src/Zoning/ZoneRegionAnalyzer.cs` (pure C#)
- New tests `tests/MedievalCityBuilder.Tests/ZoneRegionTests.cs` (and `ZoneSizeRulesTests.cs` if you prefer)
- `docs/PROGRESS.md` (append-only), `TODO.md` ("Minimum zone size" only)
- Do NOT change `ZoneGrid`, `ZoneRegistry` or `ZoneDefinition` (their data is part of the future save format; minimum sizes live in a separate rules object).

## Requirements
1. `ZoneSizeRules`: immutable; a default minimum size in cells (>= 1; PLACEHOLDER default 4) and optional per-zone overrides (zone id to minimum size, each >= 1, no override for `ZoneTypeId.None`). `GetMinSize(ZoneTypeId)`. A static `Default` labeled PLACEHOLDER. Stored in sorted order so enumeration is deterministic. Validate every argument.
2. `ZoneRegion`: zone id, cell count, cell bounding box (min and max x and z), `IsUndersized`, and an index.
3. `ZoneRegionAnalyzer.Analyze(ZoneGrid grid, ZoneSizeRules rules)` returns a `ZoneRegionMap`: 4-neighbour connected regions of the same zone id (diagonal cells are NOT connected; adjacent cells of different zones are NOT merged). Use an iterative flood fill (no recursion; it must handle a single region of 1,000,000 cells). Regions are ordered by their first cell in a row-major scan. `ZoneRegionMap` exposes `Regions`, `RegionIndexAt(x, z)` (-1 for empty cells), `IsCellInValidRegion(x, z)`, `GetRegionCells(regionIndex)` (cells in row-major order), `ValidCellCount` and `UndersizedCellCount`. Out-of-range coordinates and unknown region indexes throw `ArgumentOutOfRangeException` with the parameter name first.
4. Flag-only behavior, documented in the class comment: the analyzer reads the grid and never modifies it. Later prompts (road distance, terrain, zone search) consume `IsCellInValidRegion`.
5. Deterministic: same grid and rules give identical results. No `System.Random`.
6. Tests: connected-region detection; minimum-size boundary (exactly at the minimum is valid, one below is undersized, one above is valid); two zones touching are separate regions; diagonal cells are separate regions; several regions in one grid with the expected order; per-zone override beats the default; empty grid; a full grid of one zone; a 1,000-by-1,000 grid with one huge region (no stack overflow); region cell lists are row-major and add up to `PaintedCellCount`; rules validation (0, negative, override for None); determinism; a seeded fuzz test (`DeterministicRandom`) on 20-by-20 grids with random brush strokes, comparing the analyzer with a brute-force reference (for example union-find or a simple recursive flood fill on the small grid) for region count, region sizes and per-cell validity.

## Definition of done
- All verification steps in `_RULES.md` pass with real output; test total increased.
- `docs/PROGRESS.md` updated by append only; TODO.md "Minimum zone size" checked.

## Stop conditions
- Stop and report when complete and verified.
- Do not implement road-distance or terrain validation, and do not change painting behavior.

## Commit
`P1-4c: zone minimum size analysis (flag only)` (final commit; one commit per numbered item before it).
