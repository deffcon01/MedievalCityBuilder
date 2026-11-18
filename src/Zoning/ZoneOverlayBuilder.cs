using System;
using System.Collections.Generic;
using MedievalCityBuilder.World;

namespace MedievalCityBuilder.Zoning
{
    /// <summary>
    /// Builds a zone overlay mesh from a ZoneGrid and a terrain surface.
    /// Pure and deterministic. Each painted cell becomes one quad hugging the terrain.
    /// </summary>
    public static class ZoneOverlayBuilder
    {
        /// <summary>
        /// Builds a zone overlay mesh.
        /// </summary>
        /// <param name="grid">The zone grid to sample from.</param>
        /// <param name="surface">The terrain surface to sample heights from.</param>
        /// <param name="heightOffset">Vertical offset added to each sampled height.</param>
        /// <returns>A ZoneOverlayMesh with positions, per-vertex zone ids and indices.</returns>
        /// <exception cref="ArgumentNullException">Thrown when grid or surface is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when heightOffset is NaN or infinite.</exception>
        public static ZoneOverlayMesh Build(ZoneGrid grid, TerrainSurface surface, double heightOffset)
        {
            if (grid == null)
            {
                throw new ArgumentNullException(nameof(grid), "ZoneGrid must not be null.");
            }
            if (surface == null)
            {
                throw new ArgumentNullException(nameof(surface), "TerrainSurface must not be null.");
            }
            if (double.IsNaN(heightOffset) || double.IsInfinity(heightOffset))
            {
                throw new ArgumentOutOfRangeException(nameof(heightOffset), "HeightOffset must be a finite number.");
            }

            var positions = new List<float>();
            var zoneIds = new List<int>();
            var indices = new List<int>();
            int quadCount = 0;

            // Cells are emitted in row-major order: for each z, for each x.
            for (int z = 0; z < grid.CellsZ; z++)
            {
                for (int x = 0; x < grid.CellsX; x++)
                {
                    ZoneTypeId zone = grid.Get(x, z);
                    if (zone.IsNone)
                    {
                        continue;
                    }

                    // Cell corners in world coordinates
                    double cx0 = grid.MinX + x * grid.CellSize;
                    double cx1 = grid.MinX + (x + 1) * grid.CellSize;
                    double cz0 = grid.MinZ + z * grid.CellSize;
                    double cz1 = grid.MinZ + (z + 1) * grid.CellSize;

                    // Corner order: lower-left, lower-right, upper-right, upper-left
                    double[] cornerX = { cx0, cx1, cx1, cx0 };
                    double[] cornerZ = { cz0, cz0, cz1, cz1 };

                    // Clamp each corner inside the surface bounds before sampling height.
                    // A grid that extends past the terrain clamps instead of throwing.
                    for (int c = 0; c < 4; c++)
                    {
                        if (cornerX[c] < surface.MinX) cornerX[c] = surface.MinX;
                        else if (cornerX[c] > surface.MaxX) cornerX[c] = surface.MaxX;

                        if (cornerZ[c] < surface.MinZ) cornerZ[c] = surface.MinZ;
                        else if (cornerZ[c] > surface.MaxZ) cornerZ[c] = surface.MaxZ;
                    }

                    int baseVertex = positions.Count / 3;

                    for (int c = 0; c < 4; c++)
                    {
                        double y = surface.SampleHeight(cornerX[c], cornerZ[c]) + heightOffset;
                        positions.Add((float)cornerX[c]);
                        positions.Add((float)y);
                        positions.Add((float)cornerZ[c]);
                        zoneIds.Add(zone.Value);
                    }

                    // Two triangles per quad.
                    // Triangle 1: (base+0, base+1, base+2)
                    // Triangle 2: (base+0, base+2, base+3)
                    // Winding gives a NEGATIVE Y component for (b-a) x (c-a) on a flat quad:
                    //   T1: u=(w,0,0), v=(w,0,w) -> crossY = 0*w - w*w = -w^2 < 0
                    //   T2: u=(w,0,w), v=(0,0,w) -> crossY = w*0 - w*w = -w^2 < 0
                    indices.Add(baseVertex);
                    indices.Add(baseVertex + 1);
                    indices.Add(baseVertex + 2);

                    indices.Add(baseVertex);
                    indices.Add(baseVertex + 2);
                    indices.Add(baseVertex + 3);

                    quadCount++;
                }
            }

            return new ZoneOverlayMesh(
                positions.ToArray(),
                zoneIds.ToArray(),
                indices.ToArray(),
                quadCount);
        }
    }
}