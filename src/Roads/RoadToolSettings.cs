using System;

namespace MedievalCityBuilder.Roads
{
    /// <summary>
    /// Immutable settings for road tool.
    /// </summary>
    public sealed record RoadToolSettings
    {
        /// <summary>
        /// Snap radius in world units (must be >= 0, finite).
        /// PLACEHOLDER: 1.5 pending playtesting.
        /// </summary>
        public double SnapRadius { get; init; }

        /// <summary>
        /// Whether to continue the road chain after placement.
        /// PLACEHOLDER: true pending playtesting.
        /// </summary>
        public bool ContinueChain { get; init; }

        public RoadToolSettings(double snapRadius, bool continueChain)
        {
            if (double.IsNaN(snapRadius) || double.IsInfinity(snapRadius))
            {
                throw new ArgumentOutOfRangeException(nameof(snapRadius), "SnapRadius must be a finite number.");
            }
            if (snapRadius < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(snapRadius), "SnapRadius must be >= 0.");
            }

            SnapRadius = snapRadius;
            ContinueChain = continueChain;
        }

        /// <summary>
        /// Default road tool settings.
        /// </summary>
        public static readonly RoadToolSettings Default = new RoadToolSettings(
            snapRadius: 1.5,      // PLACEHOLDER: pending playtesting
            continueChain: true   // PLACEHOLDER: pending playtesting
        );
    }
}
