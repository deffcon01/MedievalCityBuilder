# Technical Architecture

## Engine

Godot 4.7.2 .NET.

Language: C#.

## Architecture goals

Use a modular, data-driven architecture that separates:

1. Presentation
2. Simulation/domain logic
3. World systems
4. Data/configuration
5. Persistence
6. Rendering/asset presentation

The game should be testable without requiring every test to launch the full graphical scene.

## Core rule

Simulation logic must not be embedded directly into scene nodes when it can exist as ordinary C# domain code.

Godot nodes should primarily handle:

- presentation,
- input,
- scene lifecycle,
- rendering,
- interaction,
- visual representation.

Simulation systems should operate on explicit state/data.

## Suggested structure

```text
/
├── project.godot
├── GOAL.md
├── DESIGN.md
├── ARCHITECTURE.md
├── AGENTS.md
├── TODO.md
├── docs/
├── src/
│   ├── Core/
│   ├── Simulation/
│   ├── World/
│   ├── Economy/
│   ├── Population/
│   ├── Buildings/
│   ├── Zoning/
│   ├── Roads/
│   ├── Services/
│   ├── Resources/
│   ├── Progression/
│   ├── Persistence/
│   └── Presentation/
├── scenes/
├── assets/
├── data/
├── tests/
└── tools/
```

The exact folder structure may be adjusted during implementation if the change is justified.

## Data-driven content

Building assets, zone definitions, resource definitions, service definitions, technology definitions and other tunable content should be data-driven.

Avoid hard-coding individual building assets into simulation algorithms.

A new building asset should preferably be addable by creating/configuring data rather than changing core code.

## Simulation timing

Use a hybrid simulation.

Visual presentation can update continuously.

Simulation systems should update at controlled discrete intervals.

Avoid updating expensive systems every rendered frame unless required.

The exact simulation tick frequency will be established through profiling.

## Determinism

Procedural world generation must support deterministic seeded generation.

Simulation randomness should use controlled/random streams where deterministic replay or debugging is useful.

Do not depend on incidental execution order for world generation.

## World generation modules

Separate responsibilities for:

- terrain,
- water/drainage,
- climate,
- biome/environment,
- renewable resources,
- finite deposits,
- starting-location validation,
- starting settlement.

## Save/load

Save data must preserve:

- world seed,
- generator version,
- generation settings,
- terrain modifications,
- resource depletion,
- buildings,
- roads,
- zoning,
- population,
- simulation state,
- progression,
- economy,
- kingdom state.

Never regenerate the original world and discard player modifications when loading a save.

Plan for versioned save migration.

## Performance

Do not prematurely optimize.

However, design for:

- large maps,
- many buildings,
- many citizens,
- discrete simulation ticks,
- efficient spatial queries,
- pooled/reused visual entities where appropriate,
- separation between simulation entities and rendered entities.

Measure before choosing final optimization techniques.

## Testing

Core simulation systems should have automated tests where practical.

Priority tests:

- seeded world reproducibility,
- starting-location validation,
- zoning validity,
- parcel selection,
- building asset selection,
- population/housing capacity,
- worker allocation,
- demand events,
- road validity,
- resource depletion,
- save/load,
- demolition consequences.

## Prototype-first rule

Any value explicitly marked as requiring in-game testing is a placeholder.

Do not treat design notes as final balance constants.

Create configurable values and test scenes instead.
