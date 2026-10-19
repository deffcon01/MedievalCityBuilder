using System;
using MedievalCityBuilder.World;

namespace MedievalCityBuilder.Roads
{
    /// <summary>
    /// Builds road ribbon meshes from terrain surfaces.
    /// Pure and deterministic; each segment is its own strip, so strips overlap where roads meet
    /// and junction smoothing is a later task; near the map edge the clamped edge vertices
    /// can collapse to degenerate (zero-area) triangles; the ribbon samples terrain at its
    /// own edge positions so roads tilt with slopes; display only.
    /// </summary>
    public static class RoadRibbonBuilder
    {
        /// <summary>
        /// Builds a road ribbon mesh between two points on a terrain surface.
        /// </summary>
        /// <param name="surface">The terrain surface to sample heights from.</param>
        /// <param name="x1">World X coordinate of the first endpoint.</param>
        /// <param name="z1">World Z coordinate of the first endpoint.</param>
        /// <param name="x2">World X coordinate of the second endpoint.</param>
        /// <param name="z2">World Z coordinate of the second endpoint.</param>
        /// <param name="width">Total width of the road ribbon (must be > 0).</param>
        /// <param name="spacing">Desired distance between consecutive sample points along the centerline (must be > 0).</param>
        /// <param name="heightOffset">Vertical offset added to each sampled height.</param>
        /// <returns>A RoadRibbonMesh with vertices ordered L_0, R_0, L_1, R_1, ...</returns>
        /// <exception cref="ArgumentNullException">Thrown when surface is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when any coordinate is NaN or infinite, width is NaN/infinite/<=0, spacing is NaN/infinite/<=0, heightOffset is NaN/infinite, or either endpoint is not inside surface.Contains.</exception>
        /// <exception cref="ArgumentException">Thrown when segment length < 1e-9 (zero-length segment has no direction).</exception>
        public static RoadRibbonMesh Build(
            TerrainSurface surface,
            double x1, double z1,
            double x2, double z2,
            double width,
            double spacing,
            double heightOffset)
        {
            // Validation - each before any work
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

            if (double.IsNaN(width) || double.IsInfinity(width) || width <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be a finite positive number.");
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
            double len = Math.Sqrt(dx * dx + dz * dz);

            if (len < 1e-9)
            {
                throw new ArgumentException("Segment length must be >= 1e-9 (zero-length segment has no direction).", nameof(x1));
            }

            // Sample the centerline points using RoadPolyline (only X and Z are used; Y is recomputed)
            var centerPoints = RoadPolyline.Sample(surface, x1, z1, x2, z2, spacing, heightOffset);

            // Perpendicular unit vector: px = -dz / len, pz = dx / len (same for every point, segment is straight)
            double px = -dz / len;
            double pz = dx / len;
            double half = width / 2.0;

            int pointCount = centerPoints.Count;
            int vertexCount = 2 * pointCount;
            int triangleCount = 2 * (pointCount - 1);

            var positions = new float[vertexCount * 3];
            var normals = new float[vertexCount * 3];
            var indices = new int[triangleCount * 3];

            // For each point i: left X,Z = point + (px, pz) * half; right X,Z = point - (px, pz) * half
            // Clamp each to [surface.MinX, surface.MaxX] and [surface.MinZ, surface.MaxZ]
            // Y = surface.SampleHeight(clampedX, clampedZ) + heightOffset
            // Vertices written in order L_0, R_0, L_1, R_1, ... (vertex 2*i is left, 2*i+1 is right)
            // Every normal is (0,1,0) (flat up-normals; the road material will be unshaded for now)

            for (int i = 0; i < pointCount; i++)
            {
                double cx = centerPoints[i].X;
                double cz = centerPoints[i].Z;

                // Left vertex
                double lx = cx + px * half;
                double lz = cz + pz * half;

                // Clamp to surface bounds
                if (lx < surface.MinX) lx = surface.MinX;
                else if (lx > surface.MaxX) lx = surface.MaxX;
                if (lz < surface.MinZ) lz = surface.MinZ;
                else if (lz > surface.MaxZ) lz = surface.MaxZ;

                double ly = surface.SampleHeight(lx, lz) + heightOffset;

                // Right vertex
                double rx = cx - px * half;
                double rz = cz - pz * half;

                // Clamp to surface bounds
                if (rx < surface.MinX) rx = surface.MinX;
                else if (rx > surface.MaxX) rx = surface.MaxX;
                if (rz < surface.MinZ) rz = surface.MinZ;
                else if (rz > surface.MaxZ) rz = surface.MaxZ;

                double ry = surface.SampleHeight(rx, rz) + heightOffset;

                int vi = i * 6; // 2 vertices * 3 components
                positions[vi] = (float)lx;
                positions[vi + 1] = (float)ly;
                positions[vi + 2] = (float)lz;

                positions[vi + 3] = (float)rx;
                positions[vi + 4] = (float)ry;
                positions[vi + 5] = (float)rz;

                // Flat up-normals (0, 1, 0)
                normals[vi] = 0.0f;
                normals[vi + 1] = 1.0f;
                normals[vi + 2] = 0.0f;

                normals[vi + 3] = 0.0f;
                normals[vi + 4] = 1.0f;
                normals[vi + 5] = 0.0f;
            }

            // Generate indices: for each i in 0..points.Count-2 add two triangles
            // EXACT index orders: (2i, 2i+1, 2i+2) and (2i+1, 2i+3, 2i+2)
            // Winding comment: Godot treats clockwise as front-facing; with perp = (-dz, dx) these orders
            // give a NEGATIVE Y component for (b-a) x (c-a) in every triangle, so the visible face points up
            // for any segment direction; the Y component of the cross product depends only on X and Z,
            // so terrain height cannot flip it.
            int idx = 0;
            for (int i = 0; i < pointCount - 1; i++)
            {
                int baseIdx = 2 * i;

                // Triangle 1: (2i, 2i+1, 2i+2)
                indices[idx++] = baseIdx;
                indices[idx++] = baseIdx + 1;
                indices[idx++] = baseIdx + 2;

                // Triangle 2: (2i+1, 2i+3, 2i+2)
                indices[idx++] = baseIdx + 1;
                indices[idx++] = baseIdx + 3;
                indices[idx++] = baseIdx + 2;
            }

            return new RoadRibbonMesh(positions, normals, indices);
        }
    }
}