using System;

namespace MedievalCityBuilder.World
{
    /// <summary>
    /// Immutable settings for terrain generation using fractal noise.
    /// All values are PLACEHOLDERS pending visual tuning (DESIGN.md section 2).
    /// </summary>
    public record TerrainSettings
    {
        /// <summary>
        /// Number of octaves for fractal noise. Must be >= 1.
        /// PLACEHOLDER: Exact value determined by visual tuning.
        /// </summary>
        public int Octaves { get; init; }

        /// <summary>
        /// Size in cells of the largest noise feature (base feature size). Must be > 0.
        /// PLACEHOLDER: Exact value determined by visual tuning.
        /// </summary>
        public double BaseFeatureSizeCells { get; init; }

        /// <summary>
        /// Persistence factor for amplitude reduction per octave. Must be > 0.
        /// PLACEHOLDER: Exact value determined by visual tuning.
        /// </summary>
        public double Persistence { get; init; }

        /// <summary>
        /// Lacunarity factor for frequency increase per octave. Must be > 0.
        /// PLACEHOLDER: Exact value determined by visual tuning.
        /// </summary>
        public double Lacunarity { get; init; }

        /// <summary>
        /// Creates a new TerrainSettings instance with validation.
        /// </summary>
        /// <param name="octaves">Number of octaves (must be >= 1).</param>
        /// <param name="baseFeatureSizeCells">Base feature size in cells (must be > 0).</param>
        /// <param name="persistence">Persistence factor (must be > 0).</param>
        /// <param name="lacunarity">Lacunarity factor (must be > 0).</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when any parameter is invalid.</exception>
        public TerrainSettings(int octaves, double baseFeatureSizeCells, double persistence, double lacunarity)
        {
            if (octaves < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(octaves), "Octaves must be >= 1.");
            }
            if (baseFeatureSizeCells <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseFeatureSizeCells), "BaseFeatureSizeCells must be > 0.");
            }
            if (persistence <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(persistence), "Persistence must be > 0.");
            }
            if (lacunarity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lacunarity), "Lacunarity must be > 0.");
            }

            Octaves = octaves;
            BaseFeatureSizeCells = baseFeatureSizeCells;
            Persistence = persistence;
            Lacunarity = lacunarity;
        }

        /// <summary>
        /// Default terrain settings with placeholder values pending visual tuning.
        /// </summary>
        public static readonly TerrainSettings Default = new TerrainSettings(
            octaves: 4,                        // PLACEHOLDER: pending visual tuning
            baseFeatureSizeCells: 64.0,        // PLACEHOLDER: pending visual tuning
            persistence: 0.5,                  // PLACEHOLDER: pending visual tuning
            lacunarity: 2.0                    // PLACEHOLDER: pending visual tuning
        );
    }
}