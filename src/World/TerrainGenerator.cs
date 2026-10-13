using System;

namespace MedievalCityBuilder.World
{
    /// <summary>
    /// Terrain heightmap generation using fractal value noise.
    /// Any change to this algorithm or to a stream name requires incrementing WorldGenerationSettings.CurrentGeneratorVersion.
    /// </summary>
    public static class TerrainGenerator
    {
        /// <summary>
        /// Generates a heightmap using fractal value noise.
        /// </summary>
        /// <param name="world">World generation settings including seed and version.</param>
        /// <param name="terrain">Terrain settings for fractal noise parameters.</param>
        /// <param name="width">Width in vertices (must be >= 2).</param>
        /// <param name="height">Height in vertices (must be >= 2).</param>
        /// <returns>A new Heightmap with normalized heights in [0, 1].</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when width or height is less than 2.</exception>
        public static Heightmap Generate(WorldGenerationSettings world, TerrainSettings terrain, int width, int height)
        {
            if (width < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be >= 2.");
            }
            if (height < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Height must be >= 2.");
            }

            // Derive the terrain noise seed from the world seed
            // Exactly one draw from the terrain stream
            ulong noiseSeed = RandomStreams.Derive(world.Seed, RandomStreams.Terrain).NextUInt64();

            var heightmap = new Heightmap(width, height);
            var data = heightmap.GetDataSpan();

            // Calculate total amplitude for normalization
            double totalAmplitude = 0.0;
            double amplitude = 1.0;
            double featureSize = terrain.BaseFeatureSizeCells;

            for (int octave = 0; octave < terrain.Octaves; octave++)
            {
                totalAmplitude += amplitude;
                amplitude *= terrain.Persistence;
                featureSize /= terrain.Lacunarity;
            }

            // Generate heightmap
            // Coordinates are in cells (not vertices), so we use (width-1) and (height-1) as the cell dimensions
            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Convert vertex coordinates to cell coordinates
                    // x and z range from 0 to width-1 / height-1
                    double cellX = x;
                    double cellZ = z;

                    double sum = 0.0;
                    amplitude = 1.0;
                    featureSize = terrain.BaseFeatureSizeCells;

                    for (int octave = 0; octave < terrain.Octaves; octave++)
                    {
                        // Use noiseSeed + octaveIndex for each octave to get different noise patterns
                        double sample = ValueNoise.Sample(noiseSeed + (ulong)octave, cellX / featureSize, cellZ / featureSize);
                        sum += amplitude * sample;

                        amplitude *= terrain.Persistence;
                        featureSize /= terrain.Lacunarity;
                    }

                    // Normalize by total amplitude so every value is within [0, 1]
                    double normalizedValue = sum / totalAmplitude;

                    // Clamp to [0, 1] just in case of floating point issues
                    if (normalizedValue < 0.0) normalizedValue = 0.0;
                    if (normalizedValue > 1.0) normalizedValue = 1.0;

                    data[z * width + x] = (float)normalizedValue;
                }
            }

            return heightmap;
        }

        /// <summary>
        /// Generates a heightmap using the world size from the table.
        /// </summary>
        /// <param name="world">World generation settings including seed, version, and size.</param>
        /// <param name="terrain">Terrain settings for fractal noise parameters.</param>
        /// <param name="table">World size table to get cell count from.</param>
        /// <returns>A new Heightmap with normalized heights in [0, 1].</returns>
        public static Heightmap Generate(WorldGenerationSettings world, TerrainSettings terrain, WorldSizeTable table)
        {
            int cellsPerSide = table.GetCellsPerSide(world.Size);
            // Heightmap is in vertices, so add 1 to get vertices from cells
            // Actually, wait - the spec says "width and height (in vertices)"
            // And Generate uses width/height as vertex count
            // So we need vertices = cells + 1
            return Generate(world, terrain, cellsPerSide + 1, cellsPerSide + 1);
        }
    }
}