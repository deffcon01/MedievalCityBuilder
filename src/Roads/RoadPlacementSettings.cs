using System;

namespace MedievalCityBuilder.Roads
{
    /// <summary>
    /// Immutable settings for road placement validation.
    /// All values are PLACEHOLDERS pending playtesting (DESIGN.md defers road limits to prototyping).
    /// </summary>
    public sealed record RoadPlacementSettings
    {
        /// <summary>
        /// Maximum allowed slope as rise over run (must be >= 0).
        /// PLACEHOLDER: 0.5 pending playtesting.
        /// </summary>
        public double MaxSlope { get; init; }

        /// <summary>
        /// Distance between height samples along a segment in world units (must be > 0).
        /// PLACEHOLDER: 1.0 pending playtesting.
        /// </summary>
        public double SampleSpacing { get; init; }

        /// <summary>
        /// Minimum allowed segment length in world units (must be >= 0).
        /// PLACEHOLDER: 1.0 pending playtesting.
        /// </summary>
        public double MinSegmentLength { get; init; }

        /// <summary>
        /// Creates a new RoadPlacementSettings instance with validation.
        /// </summary>
        /// <param name="maxSlope">Maximum allowed slope (rise over run), must be >= 0.</param>
        /// <param name="sampleSpacing">Distance between samples, must be > 0.</param>
        /// <param name="minSegmentLength">Minimum segment length, must be >= 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when any parameter is NaN, infinity, or out of valid range.</exception>
        public RoadPlacementSettings(double maxSlope, double sampleSpacing, double minSegmentLength)
        {
            if (double.IsNaN(maxSlope) || double.IsInfinity(maxSlope))
            {
                throw new ArgumentOutOfRangeException(nameof(maxSlope), "MaxSlope must be a finite number.");
            }
            if (maxSlope < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxSlope), "MaxSlope must be >= 0.");
            }

            if (double.IsNaN(sampleSpacing) || double.IsInfinity(sampleSpacing))
            {
                throw new ArgumentOutOfRangeException(nameof(sampleSpacing), "SampleSpacing must be a finite number.");
            }
            if (sampleSpacing <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleSpacing), "SampleSpacing must be > 0.");
            }

            if (double.IsNaN(minSegmentLength) || double.IsInfinity(minSegmentLength))
            {
                throw new ArgumentOutOfRangeException(nameof(minSegmentLength), "MinSegmentLength must be a finite number.");
            }
            if (minSegmentLength < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(minSegmentLength), "MinSegmentLength must be >= 0.");
            }

            MaxSlope = maxSlope;
            SampleSpacing = sampleSpacing;
            MinSegmentLength = minSegmentLength;
        }

        /// <summary>
        /// Default road placement settings with placeholder values.
        /// </summary>
        public static readonly RoadPlacementSettings Default = new RoadPlacementSettings(
            maxSlope: 0.5,       // PLACEHOLDER: pending playtesting
            sampleSpacing: 1.0,  // PLACEHOLDER: pending playtesting
            minSegmentLength: 1.0 // PLACEHOLDER: pending playtesting
        );
    }
}