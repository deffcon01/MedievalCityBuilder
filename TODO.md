# Development Roadmap

## Phase 0 — Project bootstrap

- [x] Create Godot 4.7.2 .NET project.
- [x] Confirm C# compilation.
- [x] Confirm Godot editor opens project.
- [x] Configure Git.
- [x] Add project documentation.
- [x] Configure OpenCode.
- [x] Create test/build commands.
- [x] Establish first working scene.

## Phase 1 — Technical prototype

### 1.1 Camera
- [x] 3D camera.
- [x] Pan.
- [x] Rotate.
- [x] Zoom.
- [x] Basic camera bounds.

### 1.2 Basic world
- [x] Bounded test terrain.
- [x] Terrain representation.
- [x] Water representation.
- [x] Basic world seed.
- [x] Save/load seed.

### 1.3 Roads
- [ ] Road path representation.
- [ ] Player road drawing.
- [ ] Terrain validity.
- [ ] Entrance snapping.
- [ ] Basic stone/gold cost.

### 1.4 Zoning
- [ ] Zone brush.
- [ ] Zone painting.
- [ ] Minimum zone size.
- [ ] Maximum distance from road/path.
- [ ] Terrain validation.
- [ ] Zone persistence.

### 1.5 Building development
- [ ] Development demand event.
- [ ] Available-zone search.
- [ ] Building footprint/parcel fitting.
- [ ] Largest-fitting-building fallback.
- [ ] Leftover-area assignment.
- [ ] Data-driven building asset definitions.
- [ ] Weighted asset selection.
- [ ] Architectural generation/legacy weighting.
- [ ] Building placement.

### 1.6 Population
- [ ] Citizen data model.
- [ ] Household data model.
- [ ] Housing capacity.
- [ ] Household creation.
- [ ] Household placement into homes.
- [ ] Homelessness.
- [ ] Basic employment.
- [ ] Basic wealth.
- [ ] Basic happiness.

## Phase 2 — First playable vertical slice

Goal:

Start a small village and allow the player to:

1. Generate a test world.
2. View the village.
3. Build a road.
4. Paint residential zoning.
5. Trigger simulated housing demand.
6. Generate different houses based on available space and household demand.
7. Add households.
8. Grow population.
9. Paint commercial zoning.
10. Generate a commercial building.
11. Assign workers.
12. Save/load the game.

This phase is the first major milestone.

## Phase 3 — Services and needs

- [ ] Food.
- [ ] Water.
- [ ] Fuel/warmth.
- [ ] Education.
- [ ] Entertainment.
- [ ] Medical service.
- [ ] Fire service.
- [ ] Village watch.
- [ ] Maintenance.
- [ ] Rat catcher.
- [ ] Lamplighter.
- [ ] Service coverage.

## Phase 4 — Economy

- [ ] Resource production.
- [ ] Resource consumption.
- [ ] General commercial economy.
- [ ] Industry.
- [ ] Local wealth.
- [ ] Kingdom trade.
- [ ] Kingdom demand.
- [ ] Export wealth.
- [ ] Visitors.

## Phase 5 — Procedural world

- [ ] Elevation.
- [ ] Water/drainage.
- [ ] Climate.
- [ ] Biomes.
- [ ] Renewable resources.
- [ ] Finite deposits.
- [ ] Starting-location validation.
- [ ] Starting village generation.
- [ ] World-size options.
- [ ] Performance profiling.

## Phase 6 — Progression

- [ ] Village levels.
- [ ] Technology tree.
- [ ] Zone unlocks.
- [ ] Special-building unlocks.
- [ ] Architectural asset unlocks.
- [ ] Wealth progression.
- [ ] Kingdom events.
- [ ] Village events.

## Phase 7 — Content

- [ ] Human assets.
- [ ] Elf assets.
- [ ] Dwarf assets.
- [ ] Residential asset families.
- [ ] Commercial asset families.
- [ ] Industrial asset families.
- [ ] Mixed-use asset families.
- [ ] Service buildings.
- [ ] Roads.
- [ ] Environment.

## Phase 8 — Endgame

- [ ] Independence conditions.
- [ ] Victory state.
- [ ] Post-victory continuation.

## Explicitly deferred until prototypes

- Exact world dimensions.
- Exact terrain resolution.
- Exact building footprints.
- Exact zone dimensions.
- Exact service radii.
- Exact simulation tick rate.
- Exact resource rates.
- Exact road maintenance rates.
- Exact population growth rates.
- Exact wealth thresholds.
- Exact happiness penalties.
- Exact construction times.
- Final art style/content quantity.
