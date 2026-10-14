using System;

namespace MedievalCityBuilder.World
{
    /// <summary>
    /// Immutable settings for the water plane.
    /// SeaLevel is a normalized height in [0, 1] relative to the heightmap.
    /// PLACEHOLDER: pending visual tuning.
    /// Rivers, lakes and groundwater are deferred to Phase 5.
    /// </summary>
    public record WaterSettings
    {
        /// <summary>
        /// Normalized sea level height in [0, 1]. A vertex is underwater when its height is strictly less than SeaLevel.
        /// </summary>
        public double SeaLevel { get; init; }

        /// <summary>
        /// Creates a new WaterSettings instance with validation.
        /// </summary>
        /// <param name="seaLevel">Normalized sea level in [0, 1].</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when SeaLevel is NaN, less than 0 or greater than 1.</exception>
        public WaterSettings(double seaLevel)
        {
            if (double.IsNaN(seaLevel))
            {
                throw new ArgumentOutOfRangeException(nameof(seaLevel), "SeaLevel must be a valid number in [0, 1]; NaN is not allowed.");
            }
            if (seaLevel < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(seaLevel), "SeaLevel must be >= 0.");
            }
            if (seaLevel > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(seaLevel), "SeaLevel must be <= 1.");
            }

            SeaLevel = seaLevel;
        }

        /// <summary>
        /// Default water settings with placeholder values.
        /// </summary>
        public static readonly WaterSettings Default = new WaterSettings(0.35); // PLACEHOLDER: pending visual tuning. Rivers, lakes and groundwater are deferred to Phase 5.
    }
}