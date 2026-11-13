using System;
using System.Collections.Generic;

namespace MedievalCityBuilder.Roads
{
    /// <summary>
    /// Minimal representation of a building entrance point for road snapping.
    /// Does not depend on any building system types.
    /// </summary>
    public readonly record struct BuildingEntrancePoint(double X, double Z, string? EntranceId = null);

    /// <summary>
    /// Interface for querying building entrance points for road snapping.
    /// </summary>
    public interface IBuildingEntranceProvider
    {
        /// <summary>
        /// Gets all active building entrance points.
        /// </summary>
        IReadOnlyList<BuildingEntrancePoint> GetEntrancePoints();
    }

    /// <summary>
    /// Result of an entrance snapping query.
    /// </summary>
    public readonly record struct EntranceSnapResult(
        bool IsSnapped,
        double X,
        double Z,
        BuildingEntrancePoint? SnappedEntrance = null
    );

    /// <summary>
    /// Pure C# domain logic for snapping candidate road coordinates to building entrances.
    /// </summary>
    public static class RoadEntranceSnapper
    {
        /// <summary>
        /// Snaps candidate coordinates (x, z) to the nearest building entrance within snapRadius.
        /// Tie behavior: If multiple entrances are at the exact same minimal distance within radius,
        /// the first encountered entrance in the provider list wins.
        /// If snapRadius &lt;= 0, entrancePoints is null/empty, or no entrance is within radius,
        /// no snapping occurs (IsSnapped = false).
        /// </summary>
        /// <param name="x">Candidate X world coordinate.</param>
        /// <param name="z">Candidate Z world coordinate.</param>
        /// <param name="entrancePoints">List of available building entrance points.</param>
        /// <param name="snapRadius">Maximum distance to snap in world units.</param>
        /// <returns>EntranceSnapResult containing whether snapping occurred and the resulting coordinates.</returns>
        public static EntranceSnapResult SnapToEntrance(
            double x,
            double z,
            IReadOnlyList<BuildingEntrancePoint>? entrancePoints,
            double snapRadius)
        {
            if (double.IsNaN(x) || double.IsInfinity(x) || double.IsNaN(z) || double.IsInfinity(z))
            {
                return new EntranceSnapResult(false, x, z);
            }

            if (snapRadius <= 0.0 || entrancePoints == null || entrancePoints.Count == 0)
            {
                return new EntranceSnapResult(false, x, z);
            }

            double maxDistSq = snapRadius * snapRadius;
            double bestDistSq = maxDistSq;
            BuildingEntrancePoint? bestEntrance = null;

            for (int i = 0; i < entrancePoints.Count; i++)
            {
                var entrance = entrancePoints[i];
                if (double.IsNaN(entrance.X) || double.IsInfinity(entrance.X) ||
                    double.IsNaN(entrance.Z) || double.IsInfinity(entrance.Z))
                {
                    continue;
                }

                double dx = entrance.X - x;
                double dz = entrance.Z - z;
                double distSq = dx * dx + dz * dz;

                // Strict inequality (< bestDistSq) preserves tie behavior: first in list wins
                if (distSq <= maxDistSq)
                {
                    if (bestEntrance == null || distSq < bestDistSq)
                    {
                        bestEntrance = entrance;
                        bestDistSq = distSq;
                    }
                }
            }

            if (bestEntrance.HasValue)
            {
                return new EntranceSnapResult(true, bestEntrance.Value.X, bestEntrance.Value.Z, bestEntrance);
            }

            return new EntranceSnapResult(false, x, z);
        }
    }
}
