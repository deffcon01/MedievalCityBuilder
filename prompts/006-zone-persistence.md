# Task 006: Save format v2, roads and zones (domain layer)

> **Read `prompts/_RULES.md` first and follow it. It overrides anything below that conflicts with it.**

## Objective
Extend the world save format to version 2 so it can store the road graph and the zone grid. This is a pure domain and serializer task (no scene code; the scene wiring is task 017). Version 1 saves must keep loading, with no roads and no zones.

## Read first
1. `AGENTS.md`, then `prompts/_RULES.md`
2. `ARCHITECTURE.md`: save/load section ("Never regenerate the original world and discard player modifications", "Plan for versioned save migration")
3. `docs/PROGRESS.md`: the save format, serializer hardening and file I/O sections
4. `src/Persistence/` (all files), especially `WorldSaveData.cs`, `WorldSaveSerializer.cs` and `SaveFormat`
5. `src/Roads/RoadGraph.cs` (ids are never reused; segments are stored with A < B) and `src/Zoning/ZoneGrid.cs`, `src/Zoning/ZoneRegistry.cs`
6. `tests/MedievalCityBuilder.Tests/WorldSaveSerializerTests.cs` and `WorldSaveFileTests.cs` (existing style)

## Allowed changes
- `src/Persistence/` (new DTO and data files, serializer changes)
- `src/Roads/RoadGraph.cs`: ONLY the additions needed to export and restore a graph (see requirement 1)
- New and extended tests; `docs/PROGRESS.md` (append-only); `TODO.md` ("Zone persistence" is checked only when task 017 is also done, so leave it unchecked here)

## Requirements
1. `RoadGraph` export and restore. Add read access to the next node id and next segment id counters, and a static factory `RoadGraph.Restore(IEnumerable<RoadNode> nodes, IEnumerable<RoadSegment> segments, int nextNodeId, int nextSegmentId)` that rebuilds the graph exactly, including the adjacency index and the rule that ids are never reused after loading. Validate strictly and throw `ArgumentException` (with the parameter name) for: duplicate node ids, duplicate segment ids, a segment that references an unknown node, a self-loop, `A.Value >= B.Value` (segments are stored normalized), a duplicate segment between the same two nodes, non-finite coordinates, ids below 1, or `nextNodeId` / `nextSegmentId` not greater than the largest existing id. Existing `RoadGraph` behavior and tests must not change.
2. `RoadSaveData` (nodes, segments, next ids) with `FromGraph(RoadGraph)` (all collections in ascending id order) and `ToGraph()`.
3. `ZoneSaveData`: the zone definitions (id, name, category), the grid geometry (min x, min z, cell size, cells x, cells z) and the cell contents as run-length encoded pairs `[zoneId, runLength]` in row-major order (so sparse grids stay small). `FromGrid(ZoneGrid)` and `ToGrid()` (which builds a `ZoneRegistry` from the saved definitions). On load validate: every cell id is 0 or a defined zone id, run lengths are positive, the runs add up to exactly `cellsX * cellsZ`, the grid size respects the existing 4,000,000 cell cap, geometry values are finite, and definitions follow the registry rules (no None id, no duplicate id or name).
4. `WorldSaveData` gains two OPTIONAL trailing members `RoadSaveData? Roads = null` and `ZoneSaveData? Zones = null`, so every existing three-argument call keeps compiling. `SaveFormat.CurrentFormatVersion` becomes 2 (update the comment: v1 had no roads or zones; v3 will add buildings, population and simulation state).
5. Serializer: `Serialize` always writes `formatVersion` 2 and writes the `roads` and `zones` sections only when present, using private DTO classes, camelCase names, explicit property order, invariant culture, ids and counts as plain numbers, node coordinates as doubles. `Deserialize` accepts format versions 1 and 2. A v1 file (hand-write a literal copy of real v1 output in the test) loads with `Roads` and `Zones` null. A v1 file that contains a `roads` or `zones` section is rejected with `SaveFormatException`. Versions above 2 are rejected with the existing "newer version" message. When a section is present it must be fully valid (use nullable DTO fields so missing fields fail, never default to 0). Wrap domain validation failures (`ArgumentException`) in `SaveFormatException` with the original as the inner exception, as the existing code does.
6. Tests: road graph round trip including a graph with removed nodes and segments (gaps in ids) and checking that ids are never reused after loading (add a node after restore and compare its id with the saved counter); an empty graph and an empty zone grid; a 5,000-node road graph; zone round trip for sparse, dense and full grids; deterministic output (serialize twice gives identical strings, sections ordered by id); v1 literal loads; v1 with roads rejected; format version 3 rejected; every invalid road and zone case from requirements 1 and 3 throws `SaveFormatException` (one test per case, including missing fields and a run-length total that is one too large and one too small); a seeded fuzz test (`DeterministicRandom`) that builds random road graphs (random adds and removals) and random painted grids, saves, loads, and compares structure exactly; the existing serializer and file tests still pass unchanged.

## Definition of done
- All verification steps in `_RULES.md` pass with real output; test total increased.
- `docs/PROGRESS.md` updated by append only, noting: roads and zones are saved from this task, the scene wiring is task 017, CellSize and HeightScale are still not saved, and a v2 save loaded by an older build is rejected.

## Stop conditions
- Stop and report when complete and verified.
- Do not touch the scene or any `src/Presentation` file.
- If restoring a graph cannot preserve the "ids are never reused" rule without a larger change to `RoadGraph`, stop and report the options.

## Commit
`P1-4g: save format v2 with roads and zones (domain layer)` (final commit; one commit per numbered item before it).
