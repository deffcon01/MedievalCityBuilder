using System;
using System.Text.Json;
using MedievalCityBuilder.World;

namespace MedievalCityBuilder.Persistence
{
    /// <summary>
    /// Converts <see cref="WorldSaveData"/> instances to and from the in-memory save string.
    /// The wire layout is produced and consumed through private DTO classes so the domain
    /// records never leak their property names into the save format.
    /// </summary>
    public static class WorldSaveSerializer
    {
        /// <summary>
        /// Serialization options. Every field is set explicitly so the output never depends
        /// on property defaults that may change between framework versions.
        /// </summary>
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>
        /// Serializes a <see cref="WorldSaveData"/> into a deterministic, culture-independent JSON string.
        /// </summary>
        public static string Serialize(WorldSaveData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var dto = new SaveDto
            {
                FormatVersion = SaveFormat.CurrentFormatVersion,
                GeneratorVersion = data.Generation.GeneratorVersion,
                Seed = data.Generation.Seed.ToString(),
                WorldSize = data.Generation.Size.ToString(),
                Terrain = new TerrainDto
                {
                    Octaves = data.Terrain.Octaves,
                    BaseFeatureSizeCells = data.Terrain.BaseFeatureSizeCells,
                    Persistence = data.Terrain.Persistence,
                    Lacunarity = data.Terrain.Lacunarity,
                },
                Water = new WaterDto
                {
                    SeaLevel = data.Water.SeaLevel,
                },
            };

            return JsonSerializer.Serialize(dto, Options);
        }

        /// <summary>
        /// Deserializes a save string into a <see cref="WorldSaveData"/>.
        /// Every failure throws <see cref="SaveFormatException"/> with a message naming the problem.
        /// Unknown extra JSON fields are ignored by the deserializer.
        /// </summary>
        public static WorldSaveData Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new SaveFormatException("Save data is null, empty or whitespace.");
            }

            SaveDto? dto;
            try
            {
                dto = JsonSerializer.Deserialize<SaveDto>(json, Options);
            }
            catch (JsonException ex)
            {
                throw new SaveFormatException("Malformed JSON: " + ex.Message, ex);
            }

            if (dto == null)
            {
                throw new SaveFormatException("JSON root must be an object.");
            }

            int formatVersion = dto.FormatVersion;
            if (formatVersion < 1)
            {
                throw new SaveFormatException("formatVersion must be >= 1; got " + formatVersion + ".");
            }

            if (formatVersion > SaveFormat.CurrentFormatVersion)
            {
                throw new SaveFormatException("save is from a newer version of the game");
            }

            int generatorVersion = dto.GeneratorVersion;
            if (generatorVersion > WorldGenerationSettings.CurrentGeneratorVersion)
            {
                throw new SaveFormatException("cannot reproduce this world with this build");
            }

            // Older generator versions are accepted for now because only version 1 exists.
            // Once the terrain generator changes, add a compatibility/migration path here.
            string? seedText = dto.Seed;
            if (seedText == null || !ulong.TryParse(seedText, out ulong seed))
            {
                throw new SaveFormatException("seed is not a valid decimal ulong: '" + seedText + "'.");
            }

            string? sizeText = dto.WorldSize;
            if (sizeText == null || !Enum.IsDefined(typeof(WorldSize), sizeText))
            {
                throw new SaveFormatException("worldSize is not a defined WorldSize name: '" + sizeText + "'.");
            }

            WorldSize size = (WorldSize)Enum.Parse(typeof(WorldSize), sizeText);

            TerrainDto? terrainDto = dto.Terrain;
            if (terrainDto == null)
            {
                throw new SaveFormatException("Missing or invalid required object field 'terrain'.");
            }

            WaterDto? waterDto = dto.Water;
            if (waterDto == null)
            {
                throw new SaveFormatException("Missing or invalid required object field 'water'.");
            }

            try
            {
                var generation = new WorldGenerationSettings(seed, generatorVersion, size);
                var terrain = new TerrainSettings(
                    terrainDto.Octaves,
                    terrainDto.BaseFeatureSizeCells,
                    terrainDto.Persistence,
                    terrainDto.Lacunarity);
                var water = new WaterSettings(waterDto.SeaLevel);
                return new WorldSaveData(generation, terrain, water);
            }
            catch (ArgumentException ex)
            {
                throw new SaveFormatException("Values rejected by domain constructors: " + ex.Message, ex);
            }
        }

        /// <summary>DTO defining the save layout. Property order is the JSON field order.</summary>
        private sealed class SaveDto
        {
            public int FormatVersion { get; set; }
            public int GeneratorVersion { get; set; }
            public string? Seed { get; set; }
            public string? WorldSize { get; set; }
            public TerrainDto? Terrain { get; set; }
            public WaterDto? Water { get; set; }
        }

        /// <summary>DTO for the terrain block. Property order is the JSON field order.</summary>
        private sealed class TerrainDto
        {
            public int Octaves { get; set; }
            public double BaseFeatureSizeCells { get; set; }
            public double Persistence { get; set; }
            public double Lacunarity { get; set; }
        }

        /// <summary>DTO for the water block. Property order is the JSON field order.</summary>
        private sealed class WaterDto
        {
            public double SeaLevel { get; set; }
        }
    }
}