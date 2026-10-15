using System;

namespace MedievalCityBuilder.Roads
{
    /// <summary>
    /// Possible failure reasons for road placement validation.
    /// </summary>
    public enum RoadPlacementFailure
    {
        /// <summary>
        /// No failure - placement is valid.
        /// </summary>
        None,

        /// <summary>
        /// One or both endpoints are outside the terrain bounds.
        /// </summary>
        OutOfBounds,

        /// <summary>
        /// Segment is shorter than the minimum allowed length.
        /// </summary>
        TooShort,

        /// <summary>
        /// Segment passes through or ends in underwater terrain.
        /// </summary>
        Underwater,

        /// <summary>
        /// Segment exceeds the maximum allowed slope.
        /// </summary>
        TooSteep
    }

    /// <summary>
    /// Result of a road placement validation check.
    /// </summary>
    public readonly record struct RoadPlacementResult
    {
        /// <summary>
        /// Whether the placement is valid.
        /// </summary>
        public bool IsValid { get; init; }

        /// <summary>
        /// The failure reason if not valid.
        /// </summary>
        public RoadPlacementFailure Failure { get; init; }

        /// <summary>
        /// Creates a valid result.
        /// </summary>
        public static RoadPlacementResult Valid => new RoadPlacementResult { IsValid = true, Failure = RoadPlacementFailure.None };

        /// <summary>
        /// Creates a failed result with the specified failure reason.
        /// </summary>
        /// <param name="failure">The failure reason.</param>
        /// <returns>A failed RoadPlacementResult.</returns>
        public static RoadPlacementResult Fail(RoadPlacementFailure failure)
        {
            return new RoadPlacementResult { IsValid = false, Failure = failure };
        }
    }
}