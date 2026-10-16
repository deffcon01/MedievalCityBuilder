using System;
using MedievalCityBuilder.World;

namespace MedievalCityBuilder.Roads
{
    /// <summary>
    /// Static validator for road placement against terrain and water.
    /// This validator checks terrain only; cost, building entrances and crossings are separate tasks.
    /// </summary>
    public static class RoadPlacementValidator
    {
        /// <summary>
        /// Validates a single road node position.
        /// </summary>
        /// <param name="surface">The terrain surface to validate against.</param>
        /// <param name="water">Water settings defining sea level.</param>
        /// <param name="x">World X coordinate of the node.</param>
        /// <param name="z">World Z coordinate of the node.</param>
        /// <returns>Valid if the node is in bounds and not underwater; otherwise the first failure found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when surface or water is null.</exception>
        public static RoadPlacementResult ValidateNode(TerrainSurface surface, WaterSettings water, double x, double z)
        {
            if (surface == null)
            {
                throw new ArgumentNullException(nameof(surface), "TerrainSurface must not be null.");
            }
            if (water == null)
            {
                throw new ArgumentNullException(nameof(water), "WaterSettings must not be null.");
            }

            if (!surface.Contains(x, z))
            {
                return RoadPlacementResult.Fail(RoadPlacementFailure.OutOfBounds);
            }

            double normalizedHeight = surface.SampleNormalized(x, z);
            if (normalizedHeight < water.SeaLevel)
            {
                return RoadPlacementResult.Fail(RoadPlacementFailure.Underwater);
            }

            return RoadPlacementResult.Valid;
        }

        /// <summary>
        /// Validates a road segment between two world coordinates.
        /// </summary>
        /// <param name="surface">The terrain surface to validate against.</param>
        /// <param name="water">Water settings defining sea level.</param>
        /// <param name="settings">Road placement settings.</param>
        /// <param name="x1">World X coordinate of the first endpoint.</param>
        /// <param name="z1">World Z coordinate of the first endpoint.</param>
        /// <param name="x2">World X coordinate of the second endpoint.</param>
        /// <param name="z2">World Z coordinate of the second endpoint.</param>
        /// <returns>The first failure found along the path from point 1 to point 2, or Valid if none.</returns>
        /// <exception cref="ArgumentNullException">Thrown when surface, water, or settings is null.</exception>
        public static RoadPlacementResult ValidateSegment(
            TerrainSurface surface,
            WaterSettings water,
            RoadPlacementSettings settings,
            double x1, double z1,
            double x2, double z2)
        {
            if (surface == null)
            {
                throw new ArgumentNullException(nameof(surface), "TerrainSurface must not be null.");
            }
            if (water == null)
            {
                throw new ArgumentNullException(nameof(water), "WaterSettings must not be null.");
            }
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings), "RoadPlacementSettings must not be null.");
            }

            // Check both endpoints in bounds
            if (!surface.Contains(x1, z1) || !surface.Contains(x2, z2))
            {
                return RoadPlacementResult.Fail(RoadPlacementFailure.OutOfBounds);
            }

            // Calculate segment length
            double dx = x2 - x1;
            double dz = z2 - z1;
            double length = Math.Sqrt(dx * dx + dz * dz);

            // Check minimum segment length
            if (length < settings.MinSegmentLength)
            {
                return RoadPlacementResult.Fail(RoadPlacementFailure.TooShort);
            }

            // Determine number of samples: ceil(length / SampleSpacing)
            // Include both endpoints, so we need at least 1 step (2 samples for length > 0)
            // For length == 0 and MinSegmentLength == 0, we still need to check the single point
            int numSteps;
            if (length <= 0.0)
            {
                numSteps = 0; // Single point at the start
            }
            else
            {
                double stepsD = Math.Ceiling(length / settings.SampleSpacing);
                if (stepsD > 1000000.0)
                {
                    stepsD = 1000000.0; // protects against tiny spacing values
                }
                numSteps = (int)stepsD;
                if (numSteps < 1)
                {
                    numSteps = 1;
                }
            }

            // Sample along the segment from point 1 to point 2
            double prevHeight = surface.SampleHeight(x1, z1);
            double prevNorm = surface.SampleNormalized(x1, z1);

            // Check first sample for underwater
            if (prevNorm < water.SeaLevel)
            {
                return RoadPlacementResult.Fail(RoadPlacementFailure.Underwater);
            }

            // Walk the segment
            for (int i = 1; i <= numSteps; i++)
            {
                double t = (double)i / numSteps;
                double sx, sz;
                if (i == numSteps)
                {
                    sx = x2;
                    sz = z2;
                }
                else
                {
                    sx = x1 * (1.0 - t) + x2 * t;
                    sz = z1 * (1.0 - t) + z2 * t;
                }

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

                double currNorm = surface.SampleNormalized(sx, sz);

                // Check underwater at each sample
                if (currNorm < water.SeaLevel)
                {
                    return RoadPlacementResult.Fail(RoadPlacementFailure.Underwater);
                }

                // Check slope against previous sample
                double currHeight = surface.SampleHeight(sx, sz);
                double heightDiff = Math.Abs(currHeight - prevHeight);
                double sampleDist = length / numSteps; // distance between consecutive samples
                double slope = heightDiff / sampleDist;

                if (slope > settings.MaxSlope)
                {
                    return RoadPlacementResult.Fail(RoadPlacementFailure.TooSteep);
                }

                prevHeight = currHeight;
            }

            return RoadPlacementResult.Valid;
        }

        /// <summary>
        /// Validates a road segment between two nodes in a RoadGraph.
        /// </summary>
        /// <param name="surface">The terrain surface to validate against.</param>
        /// <param name="water">Water settings defining sea level.</param>
        /// <param name="settings">Road placement settings.</param>
        /// <param name="graph">The road graph containing the nodes.</param>
        /// <param name="a">ID of the first node.</param>
        /// <param name="b">ID of the second node.</param>
        /// <returns>The validation result for the segment between the two nodes.</returns>
        /// <exception cref="ArgumentNullException">Thrown when surface, water, settings, or graph is null.</exception>
        /// <exception cref="ArgumentException">Propagated from graph.GetNode if either node ID is unknown.</exception>
        public static RoadPlacementResult ValidateSegment(
            TerrainSurface surface,
            WaterSettings water,
            RoadPlacementSettings settings,
            RoadGraph graph,
            RoadNodeId a,
            RoadNodeId b)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph), "RoadGraph must not be null.");
            }

            var nodeA = graph.GetNode(a);
            var nodeB = graph.GetNode(b);

            return ValidateSegment(surface, water, settings, nodeA.X, nodeA.Z, nodeB.X, nodeB.Z);
        }
    }
}