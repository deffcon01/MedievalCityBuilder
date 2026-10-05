# Game Design Specification

## 1. Camera

- 3D world.
- Isometric-style presentation.
- Camera can move, rotate and zoom.
- Exact camera angle and limits are prototype decisions.

## 2. World

### Boundaries

Maps are bounded.

Player selects:

- Small
- Medium
- Large

Exact dimensions/resolution are determined through performance testing.

### Deterministic generation

World generation is seed-based.

For a fixed:

- seed,
- generation settings,
- generator version,

the initial generated world must be reproducible.

Save files must preserve player modifications and must not regenerate the world destructively.

### Terrain

Natural-looking 3D terrain containing combinations of:

- plains,
- rolling hills,
- valleys,
- mountains/foothills,
- rivers,
- lakes,
- forests,
- grasslands,
- natural obstacles.

Rivers should follow plausible downhill paths.

### Terrain modification

Full terraforming is planned:

- raise,
- lower,
- flatten,
- create building sites,
- modify slopes.

Terraforming must not corrupt:

- water,
- deposits,
- building foundations,
- roads,
- save state,
- simulation state.

Exact tools, costs and limits are deferred until prototyping.

### Climate

Climate varies gradually.

Initial broad climate/biome families:

- temperate woodland,
- grassland,
- cold highlands,
- arid environments.

Climate is influenced by world-scale parameters, elevation, moisture, precipitation and bodies of water.

Climate affects:

- crops,
- forest growth,
- heating requirements,
- water availability,
- seasonal weather,
- land suitability.

Biomes should emerge from environmental conditions rather than being arbitrary isolated patches.

## 3. Starting village

The starting village is randomly positioned subject to minimum viability rules.

It begins with:

- village hall,
- several houses,
- connected basic roads,
- 8–12 citizens,
- human population,
- housing for starting population,
- basic food/water/fuel availability,
- viable nearby expansion land.

Starting location must have:

- sufficient buildable land,
- viable water access,
- a way to produce or acquire essentials,
- terrain suitable for connectivity,
- no unavoidable geographic trap.

Generation must use bounded retries and a fallback/failure path. Never use an infinite search loop.

## 4. Resources

### Renewable

Examples:

- forests,
- crops,
- vegetation.

Regeneration depends on climate, land and harvesting pressure.

### Finite

Examples:

- stone,
- coal,
- iron ore,
- other minerals.

Finite deposits track remaining quantity and stop production when exhausted.

### Water

Water can come from:

- rivers,
- lakes,
- groundwater,
- wells.

The exact settlement water model remains a prototype decision.

### Trade

Some shortages can be resolved through trade or expansion.

Trade availability and prices depend on the kingdom economy.

## 5. Zoning

Zoning is a paint-brush system.

The player paints areas rather than manually placing ordinary buildings.

Each zone has:

- minimum sizing rules,
- maximum distance from a valid path/road,
- terrain suitability requirements.

Zoning is free.

Ordinary buildings generated inside zones have no direct construction resource cost.

### Starting zoning unlocks

The village starts with:

- Small Residential
- Multifamily Residential
- Small Open Market
- Light Commercial
- Light Industry
- Residential-Agriculture
- Residential-Industry
- Residential-Commercial

Additional zoning types unlock as the village progresses.

### Residential zones

| Zone | Target population | Notes |
|---|---:|---|
| Small Residential | 3–6 | Normal density |
| Multifamily Residential | 6–12 | Higher density; happiness penalty |
| Large Residential | 4–8 | Normal density |
| Small Manor Residential | 5–10 | Lower density; wealth requirement |
| Large Manor Residential | 6–14 | Lower density; higher wealth requirement |
| Estate Residential | 6–20 | Lowest density; highest wealth requirement |

Manor/estate buildings also provide employment. Exact employment assignment for all manor tiers remains to be clarified during population design.

### Mixed-use zones

Residential-Agriculture:

- 4–8 residents.
- Does not require normal external food access to support population growth.

Residential-Industry:

- 3–5 residents.
- Adds finished resources available for kingdom trade.

Residential-Commercial:

- 3–5 residents.
- Adds family wealth and contributes to the local economy.

### Commercial zones

Starting:

- Small Open Market
- Light Commercial

Later:

- Large Open Market
- Medium Commercial
- Large Commercial

Commercial buildings can be:

- resource/input dependent businesses,
- general economic businesses that primarily generate gold/activity.

Workers are determined by parcel size:

| Parcel | Workers |
|---|---:|
| Small | 2–3 |
| Medium | 3–5 |
| Large | 5–10 |

Higher commercial zone level uses:

`ceil(base_workers * 1.75)`

Commercial activity keeps wealth circulating locally.

Outside visitors provide a planned +10% economic benefit to commercial activity when applicable.

### Industrial zones

Starting:

- Light Industry

Later:

- Moderate Industry
- Heavy Industry

Industry can be:

- specific resource-processing/production,
- general industry primarily generating economic activity.

Workers:

| Parcel | Workers |
|---|---:|
| Small | 3–4 |
| Medium | 6–9 |
| Large | 13–15 |

Higher industrial zone level uses:

`ceil(base_workers * 1.75)`

Industry generates planned wealth based on exports to the kingdom.

Initial target:

`50% wealth contribution based on exported resources`

Kingdom resource needs are generated by the kingdom simulation and change over time.

## 6. Parcel generation

Players do not create individual parcels.

Process:

1. Player paints zoning.
2. Simulation creates a development demand event.
3. Simulation selects the required building category.
4. Building selection determines required footprint/parcel size.
5. Game checks available zoned space.
6. If selected building does not fit, choose the largest eligible building of that zone type that fits.
7. If remaining zoned area is too small for another valid building, assign it to the last generated building.
8. Develop the building.

Exact building dimensions must be determined by actual Godot prototype testing.

Do not hard-code final dimensions before testing.

## 7. Development events

Empty zoning does not automatically generate buildings.

Buildings are generated when the simulation requests development.

Examples:

- new citizens need housing,
- new household arrives,
- new business opens,
- commercial demand increases,
- industrial demand increases,
- other defined simulation events create development demand.

## 8. Building asset system

Ordinary buildings are selected from data-driven asset pools.

Every asset should have metadata describing things such as:

- zone type,
- footprint class/size,
- capacity,
- architectural generation,
- climate suitability,
- terrain suitability,
- wealth suitability,
- unlock condition,
- visual style/family,
- selection weight.

### Architectural progression

New unlocks add new assets to the eligible pool.

Old assets should not immediately disappear.

Use conceptual states:

- Active/current assets: high selection weight.
- Legacy assets: lower selection weight.
- Retired assets: no longer naturally generated, but existing buildings remain.

Existing buildings should retain their generated architectural identity.

This creates:

- older village centers,
- transitional neighborhoods,
- newer outer neighborhoods.

### Asset families

Assets can belong to visual families.

Example:

- Timber Cottage family
- Stone House family
- Merchant House family

Selection should use weighted randomness and discourage excessive repetition.

Nearby buildings can influence style selection to encourage neighborhood continuity without forcing identical buildings.

## 9. Residential building selection

The game should not directly map:

`zone size -> fixed building`

Instead:

`zone + available space + population demand + household characteristics + simulation conditions -> eligible asset pool -> weighted selection`

Example:

A zone large enough for either:

- one larger family home,
- or two smaller homes,

may select either outcome based on current household demand, asset availability and weighted randomness.

Household characteristics should influence selection.

Examples:

- larger families favor larger homes,
- smaller households can use smaller homes/multifamily,
- wealthy households favor manor/estate housing.

## 10. Building progression

Buildings do not physically expand.

They may receive visual changes based on occupant wealth.

Higher wealth can result in a more developed visual state.

A building can be physically replaced only if:

- demolished,
- destroyed,
- or becomes unoccupied for a defined period and is redeveloped.

## 11. Roads

Roads are directly placed by the player.

Roads:

- cost gold and stone,
- automatically snap to building entrances when close,
- require supporting terrain,
- cannot cross excessively steep/rough terrain without clearing/terraforming.

Road maintenance consumes stone over time.

Maintenance consumption increases with road usage.

## 12. Special buildings

Special buildings are placed directly by the player.

Examples:

- village watch/police,
- fire watch/fire station,
- schools,
- chapels,
- doctor/medical buildings,
- maintenance buildings,
- rat catcher hut,
- lamplighter hut,
- other civic/upkeep/service buildings.

Special buildings:

- cost gold and resources,
- have terrain requirements,
- require path/road connection where applicable,
- have construction time,
- have service areas or village-wide effects depending on building.

Exact footprints and service ranges are prototype values.

## 13. Construction

Roads and special buildings require:

- gold,
- applicable resources,
- construction time.

Zoning is free.

Simulation-generated ordinary buildings have no direct player construction cost.

Construction timing should initially be inspired by conventional city-builder pacing, then balanced through playtesting.

## 14. Demolition

Player-built buildings:

- demolition costs 10% of their gold construction cost,
- refunds 40% of construction resources.

Simulation-generated buildings:

- demolition costs nothing,
- refunds nothing.

Demolition immediately leaves a flat cleared parcel that can be redeveloped very quickly.

### Residential demolition

- occupants immediately become homeless,
- causes a small immediate happiness impact,
- households seek available housing or future housing.

### Commercial/industrial demolition

- affects workers who lose jobs,
- impacts associated economic activity,
- does not create an artificial demolition penalty beyond simulation consequences.

## 15. Population

Population system is not yet fully specified.

Required concepts:

- citizens,
- households,
- housing capacity,
- homelessness,
- employment,
- unemployment,
- wealth,
- needs,
- happiness,
- education,
- species.

Initial population is human.

Species planned:

- Humans: common baseline.
- Elves: rarer, higher-end food, better housing, magical education.
- Dwarves: rarer, industry and entertainment emphasis.

## 16. Citizen needs

Planned needs:

- housing,
- food,
- education,
- warmth/fuel during cold seasons,
- employment,
- entertainment.

Missing needs reduce happiness.

Severe shortages can cause:

- citizens leaving,
- deaths,
- reduced growth,
- economic problems.

## 17. Economy

There are two interacting economies.

### Village economy

Driven by:

- population,
- jobs,
- production,
- consumption,
- local wealth,
- resources,
- services,
- seasons/weather.

### Kingdom economy

Generated at world start and simulated over time.

It can experience:

- war,
- famine,
- civil war,
- prosperity,
- anarchy,
- other kingdom-scale events.

Kingdom events affect kingdom simulation and trade rather than directly becoming graphical village events.

## 18. Progression

Progression is driven by:

- population milestones,
- building milestones,
- special-building milestones,
- technology requirements,
- village development.

Technology unlocks:

- new zoning,
- new services,
- new buildings/assets,
- other progression content.

## 19. Events

Village events can include:

- natural disasters,
- bandit raids,
- other local events.

There is no direct combat system initially.

Guards/training centers modify the effects of raids.

## 20. Victory

Victory is achieved by:

- completing the technology tree,
- constructing required special buildings,
- meeting an independence condition.

Independence can be achieved through:

- sufficient combat strength,
- or sufficient economic size.

Victory does not end gameplay.
