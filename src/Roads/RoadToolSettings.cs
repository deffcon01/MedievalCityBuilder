using System;

namespace MedievalCityBuilder.Roads
{
    /// <summary>
    /// Immutable settings for road tool.
    /// </summary>
    public sealed record RoadToolSettings
    {
        /// <summary>
        /// Snap radius in world units for snapping to existing road nodes (must be >= 0, finite).
        /// PLACEHOLDER: 1.5 pending playtesting.
        /// </summary>
        public double SnapRadius { get; init; }

        /// <summary>
        /// Snap radius in world units for snapping to building entrances (must be >= 0, finite).
        /// PLACEHOLDER: 1.5 pending playtesting.
        /// </summary>
        public double EntranceSnapRadius { get; init; }

        /// <summary>
        /// Whether to continue the road chain after placement.
        /// PLACEHOLDER: true pending playtesting.
        /// </summary>
        public bool ContinueChain { get; init; }

        /// <summary>
        /// Creates road tool settings with separate snap radii for road nodes and building entrances.
        /// </summary>
        public RoadToolSettings(double snapRadius, double entranceSnapRadius, bool continueChain)
        {
            if (double.IsNaN(snapRadius) || double.IsInfinity(snapRadius))
            {
                throw new ArgumentOutOfRangeException(nameof(snapRadius), "SnapRadius must be a finite number.");
            }
            if (snapRadius < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(snapRadius), "SnapRadius must be >= 0.");
            }
            if (double.IsNaN(entranceSnapRadius) || double.IsInfinity(entranceSnapRadius))
            {
                throw new ArgumentOutOfRangeException(nameof(entranceSnapRadius), "EntranceSnapRadius must be a finite number.");
            }
            if (entranceSnapRadius < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(entranceSnapRadius), "EntranceSnapRadius must be >= 0.");
            }

            SnapRadius = snapRadius;
            EntranceSnapRadius = entranceSnapRadius;
            ContinueChain = continueChain;
        }

        /// <summary>
        /// Creates road tool settings with the same radius for both node and entrance snapping.
        /// </summary>
        public RoadToolSettings(double snapRadius, bool continueChain)
            : this(snapRadius, snapRadius, continueChain)
        {
        }

        /// <summary>
        /// Default road tool settings.
        /// </summary>
        public static readonly RoadToolSettings Default = new RoadToolSettings(
            snapRadius: 1.5,           // PLACEHOLDER: pending playtesting
            entranceSnapRadius: 1.5,   // PLACEHOLDER: pending playtesting
            continueChain: true        // PLACEHOLDER: pending playtesting
        );
    }
}
