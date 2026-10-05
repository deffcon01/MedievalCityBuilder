# Game Goal

## High-level goal

Create a single-player medieval fantasy city-building game in which the player starts with a tiny village and develops it into a large, prosperous settlement and eventually an independent city.

The game should emphasize:

- Organic city growth.
- Procedural worlds.
- Player-directed zoning and infrastructure.
- Simulation-driven building generation.
- Population and household needs.
- A local village economy connected to a simulated kingdom economy.
- A visually evolving settlement where old buildings remain while newer architectural styles appear as the settlement progresses.
- A large 3D world viewed through an isometric-style movable, rotatable and zoomable camera.

The player should feel that they are guiding the development of a living settlement rather than manually placing every house.

## Core design principle

The player controls the city's structure and priorities.

The simulation controls the individual ordinary buildings and citizens.

The player directly places:

- Roads.
- Special/service buildings.
- Zoning.

The simulation generates ordinary:

- Houses.
- Apartments.
- Markets.
- Commercial buildings.
- Industrial buildings.
- Mixed-use buildings.

## Core gameplay loop

1. Generate a viable procedural world.
2. Start with a small village, village hall, several homes, roads and 8–12 citizens.
3. Provide housing, food, water, fuel, jobs, education, entertainment and services.
4. Paint zoning to indicate where future development is permitted.
5. Build and maintain roads and special buildings.
6. Respond to population and economic demand.
7. Allow the simulation to generate buildings and households.
8. Unlock additional zoning types, technologies and architectural assets.
9. Expand the settlement.
10. Manage local resources and trade with the kingdom.
11. Survive events and changing kingdom conditions.
12. Complete the technology tree and satisfy the independence conditions.
13. Continue playing after victory without additional progression requirements.

## End state

The player can achieve independence from the kingdom through the game's defined military-strength or economic-size condition.

Victory does not end the save.

The player may continue building and managing the city after independence.

## Design constraints

- Single player initially.
- No multiplayer requirement.
- Offline/local gameplay is not a requirement for this desktop game.
- Simulation should support large maps and large settlements.
- Do not overcomplicate simulation where a simpler model produces the intended gameplay.
- Prefer data-driven systems so content can be added without rewriting core systems.
- Prototype uncertain numbers rather than pretending they are finalized.

## Current implementation priority

Do not start with final art or complete procedural generation.

Build the smallest playable simulation that proves:

- terrain/world representation,
- camera,
- roads,
- zoning,
- simulation demand events,
- parcel/building selection,
- population,
- basic housing,
- asset-pool selection,
- save/load.

Then expand the simulation.
