using System;
using System.Collections.Generic;
using MedievalCityBuilder.World;

namespace MedievalCityBuilder.Roads
{
    /// <summary>
    /// A point on a road polyline with world coordinates (X, Y, Z).
    /// </summary>
    /// <param name="X">World X coordinate.</param>
    /// <param name="Y">World Y coordinate (height).</param>
    /// <param name="Z">World Z coordinate.</param>
    public readonly record struct RoadPolylinePoint(double X, double Y, double Z);

    /// <summary>
    /// Pure and deterministic terrain-following polyline sampling for road display.
    /// The points follow the terrain surface so a drawn road hugs hills; used for display only.
    /// </summary>
    public static class RoadPolyline
    {
        /// <summary>
        /// Samples a terrain-following polyline between two world coordinates.
        /// </summary>
        /// <param name="surface">The terrain surface to sample heights from.</param>
        /// <param name="x1">World X coordinate of the first endpoint.</param>
        /// <param name="z1">World Z coordinate of the first endpoint.</param>
        /// <param name="x2">World X coordinate of the second endpoint.</param>
        /// <param name="z2">World Z coordinate of the second endpoint.</param>
        /// <param name="spacing">Desired distance between consecutive sample points (must be > 0).</param>
        /// <param name="heightOffset">Vertical offset added to each sampled height.</param>
        /// <returns>A read-only list of points following the terrain, including both endpoints.</returns>
        /// <exception cref="ArgumentNullException">Thrown when surface is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when any coordinate is NaN or infinite, spacing is NaN, infinite, or <= 0, heightOffset is NaN or infinite, or either endpoint is not inside surface.Contains.</exception>
        public static IReadOnlyList<RoadPolylinePoint> Sample(
            TerrainSurface surface,
            double x1, double z1,
            double x2, double z2,
            double spacing,
            double heightOffset)
        {
            if (surface == null)
            {
                throw new ArgumentNullException(nameof(surface), "TerrainSurface must not be null.");
            }

            if (double.IsNaN(x1) || double.IsInfinity(x1) ||
                double.IsNaN(z1) || double.IsInfinity(z1) ||
                double.IsNaN(x2) || double.IsInfinity(x2) ||
                double.IsNaN(z2) || double.IsInfinity(z2))
            {
                throw new ArgumentOutOfRangeException(nameof(x1), "Coordinates must be finite numbers.");
            }

            if (double.IsNaN(spacing) || double.IsInfinity(spacing) || spacing <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(spacing), "Spacing must be a finite positive number.");
            }

            if (double.IsNaN(heightOffset) || double.IsInfinity(heightOffset))
            {
                throw new ArgumentOutOfRangeException(nameof(heightOffset), "HeightOffset must be a finite number.");
            }

            if (!surface.Contains(x1, z1))
            {
                throw new ArgumentOutOfRangeException(nameof(x1), "First endpoint is outside terrain bounds.");
            }

            if (!surface.Contains(x2, z2))
            {
                throw new ArgumentOutOfRangeException(nameof(x2), "Second endpoint is outside terrain bounds.");
            }

            double dx = x2 - x1;
            double dz = z2 - z1;
            double length = Math.Sqrt(dx * dx + dz * dz);

            var points = new List<RoadPolylinePoint>();

            if (length == 0.0)
            {
                double y = surface.SampleHeight(x1, z1) + heightOffset;
                points.Add(new RoadPolylinePoint(x1, y, z1));
                return points;
            }

            double stepsD = Math.Ceiling(length / spacing);
            if (stepsD > 100000.0)
            {
                stepsD = 100000.0; // protects against tiny spacing values
            }
            int steps = (int)stepsD;
            if (steps < 1)
            {
                steps = 1;
            }

            for (int i = 0; i <= steps; i++)
            {
                double t = (double)i / steps;
                double sx, sz;

                if (i == 0)
                {
                    sx = x1;
                    sz = z1;
                }
                else if (i == steps)
                {
                    sx = x2;
                    sz = z2;
                }
                else
                {
                    sx = x1 * (1.0 - t) + x2 * t;
                    sz = z1 * (1.0 - t) + z2 * t;
                }

                // Clamp to surface bounds
                if (sx < surface.MinX)
                {
                    sx = surface.MinX;
                }
                else if (sx > surface.MaxX)
                {
                    sx = surface.MaxX;
                }

                if (sz < surface.MinZ)
                {
                    sz = surface.MinZ;
                }
                else if (sz > surface.MaxZ)
                {
                    sz = surface.MaxZ;
                }

                double y = surface.SampleHeight(sx, sz) + heightOffset;
                points.Add(new RoadPolylinePoint(sx, y, sz));
            }

            return points;
        }
    }
}