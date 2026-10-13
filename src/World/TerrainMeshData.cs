using System;

namespace MedievalCityBuilder.World
{
    /// <summary>
    /// Holds terrain mesh data generated from a heightmap.
    /// Pure C# domain type with no Godot dependencies.
    /// </summary>
    public sealed class TerrainMeshData
    {
        /// <summary>
        /// Vertex positions in world coordinates. Layout: [x0, y0, z0, x1, y1, z1, ...].
        /// </summary>
        public float[] Positions { get; }

        /// <summary>
        /// Vertex normals in world coordinates. Layout: [nx0, ny0, nz0, nx1, ny1, nz1, ...].
        /// </summary>
        public float[] Normals { get; }

        /// <summary>
        /// Triangle indices (3 per triangle, clockwise winding for front face pointing up).
        /// </summary>
        public int[] Indices { get; }

        /// <summary>
        /// Number of vertices.
        /// </summary>
        public int VertexCount => Positions.Length / 3;

        /// <summary>
        /// Number of triangles.
        /// </summary>
        public int TriangleCount => Indices.Length / 3;

        private TerrainMeshData(float[] positions, float[] normals, int[] indices)
        {
            Positions = positions;
            Normals = normals;
            Indices = indices;
        }

        /// <summary>
        /// Builds terrain mesh data from a heightmap.
        /// </summary>
        /// <param name="heightmap">The heightmap to build from. Must not be null.</param>
        /// <param name="cellSize">World size of one cell in X and Z. Must be > 0.</param>
        /// <param name="heightScale">Vertical scale factor for height values. Must be >= 0.</param>
        /// <returns>A new TerrainMeshData instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when heightmap is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when cellSize <= 0 or heightScale < 0.</exception>
        public static TerrainMeshData Build(Heightmap heightmap, double cellSize, double heightScale)
        {
            if (heightmap == null)
            {
                throw new ArgumentNullException(nameof(heightmap));
            }
            if (cellSize <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(cellSize), "cellSize must be > 0.");
            }
            if (heightScale < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(heightScale), "heightScale must be >= 0.");
            }

            int width = heightmap.Width;
            int height = heightmap.Height;
            int vertexCount = width * height;
            int cellCountX = width - 1;
            int cellCountZ = height - 1;
            int triangleCount = cellCountX * cellCountZ * 2;

            var positions = new float[vertexCount * 3];
            var normals = new float[vertexCount * 3];
            var indices = new int[triangleCount * 3];

            // Compute positions
            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    int vi = (z * width + x) * 3;
                    float worldX = (float)(x * cellSize);
                    float worldY = heightmap.Get(x, z) * (float)heightScale;
                    float worldZ = (float)(z * cellSize);

                    positions[vi] = worldX;
                    positions[vi + 1] = worldY;
                    positions[vi + 2] = worldZ;
                }
            }

            // Compute normals using central differences (one-sided at edges)
            // n = normalize(-dh/dx, 1, -dh/dz)
            double invCellSize = 1.0 / cellSize;
            double invHeightScale = heightScale > 0.0 ? 1.0 / heightScale : 0.0;

            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    double dhdx, dhdz;

                    // dh/dx: central difference, one-sided at edges
                    if (x == 0)
                    {
                        // Forward difference at left edge
                        dhdx = (heightmap.Get(x + 1, z) - heightmap.Get(x, z)) * invCellSize;
                    }
                    else if (x == width - 1)
                    {
                        // Backward difference at right edge
                        dhdx = (heightmap.Get(x, z) - heightmap.Get(x - 1, z)) * invCellSize;
                    }
                    else
                    {
                        // Central difference
                        dhdx = (heightmap.Get(x + 1, z) - heightmap.Get(x - 1, z)) * 0.5 * invCellSize;
                    }

                    // dh/dz: central difference, one-sided at edges
                    if (z == 0)
                    {
                        // Forward difference at front edge
                        dhdz = (heightmap.Get(x, z + 1) - heightmap.Get(x, z)) * invCellSize;
                    }
                    else if (z == height - 1)
                    {
                        // Backward difference at back edge
                        dhdz = (heightmap.Get(x, z) - heightmap.Get(x, z - 1)) * invCellSize;
                    }
                    else
                    {
                        // Central difference
                        dhdz = (heightmap.Get(x, z + 1) - heightmap.Get(x, z - 1)) * 0.5 * invCellSize;
                    }

                    // Normal: (-dh/dx, 1/heightScale, -dh/dz) normalized
                    // The 1/heightScale factor converts the vertical component to match the scaled positions
                    double nx = -dhdx * invHeightScale;
                    double ny = 1.0;
                    double nz = -dhdz * invHeightScale;

                    double len = Math.Sqrt(nx * nx + ny * ny + nz * nz);
                    if (len > 0.0)
                    {
                        nx /= len;
                        ny /= len;
                        nz /= len;
                    }

                    int ni = (z * width + x) * 3;
                    normals[ni] = (float)nx;
                    normals[ni + 1] = (float)ny;
                    normals[ni + 2] = (float)nz;
                }
            }

            // Generate indices: two triangles per cell
            // Winding: clockwise when viewed from front (visible face points up)
            // In right-handed Y-up: for triangle (a,b,c), Y component of (b-a) x (c-a) must be NEGATIVE
            int idx = 0;
            for (int z = 0; z < cellCountZ; z++)
            {
                for (int x = 0; x < cellCountX; x++)
                {
                    int v00 = z * width + x;
                    int v10 = z * width + (x + 1);
                    int v01 = (z + 1) * width + x;
                    int v11 = (z + 1) * width + (x + 1);

                    // Triangle 1: v00, v10, v01 (clockwise from above = front face up)
                    // (v10 - v00) x (v01 - v00) should have negative Y
                    indices[idx++] = v00;
                    indices[idx++] = v10;
                    indices[idx++] = v01;

                    // Triangle 2: v10, v11, v01 (clockwise from above = front face up)
                    // (v11 - v10) x (v01 - v10) should have negative Y
                    indices[idx++] = v10;
                    indices[idx++] = v11;
                    indices[idx++] = v01;
                }
            }

            return new TerrainMeshData(positions, normals, indices);
        }
    }
}