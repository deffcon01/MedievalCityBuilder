using System;

namespace MedievalCityBuilder.Roads
{
    /// <summary>
    /// A pure, deterministic road ribbon mesh for display only.
    /// Each segment is its own strip, so strips overlap where roads meet; junction smoothing is a later task.
    /// Near the map edge the clamped edge vertices can collapse to degenerate (zero-area) triangles.
    /// The ribbon samples terrain at its own edge positions so roads tilt with slopes.
    /// </summary>
    public sealed class RoadRibbonMesh
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
        /// Triangle indices (3 per triangle).
        /// </summary>
        public int[] Indices { get; }

        /// <summary>
        /// Number of vertices (Positions.Length / 3).
        /// </summary>
        public int VertexCount => Positions.Length / 3;

        /// <summary>
        /// Number of triangles (Indices.Length / 3).
        /// </summary>
        public int TriangleCount => Indices.Length / 3;

        /// <summary>
        /// Creates a new RoadRibbonMesh.
        /// </summary>
        /// <param name="positions">Vertex positions (x, y, z per vertex). Must not be null, length must be divisible by 3.</param>
        /// <param name="normals">Vertex normals (x, y, z per vertex). Must not be null, length must equal Positions.Length.</param>
        /// <param name="indices">Triangle indices (3 per triangle). Must not be null, length must be divisible by 3.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="ArgumentException">Thrown when Positions.Length != Normals.Length, Positions.Length % 3 != 0, or Indices.Length % 3 != 0.</exception>
        public RoadRibbonMesh(float[] positions, float[] normals, int[] indices)
        {
            if (positions == null)
            {
                throw new ArgumentNullException(nameof(positions), "Positions must not be null.");
            }
            if (normals == null)
            {
                throw new ArgumentNullException(nameof(normals), "Normals must not be null.");
            }
            if (indices == null)
            {
                throw new ArgumentNullException(nameof(indices), "Indices must not be null.");
            }

            if (positions.Length != normals.Length)
            {
                throw new ArgumentException("Positions and Normals must have the same length.", nameof(normals));
            }
            if (positions.Length % 3 != 0)
            {
                throw new ArgumentException("Positions length must be a multiple of 3 (x, y, z per vertex).", nameof(positions));
            }
            if (indices.Length % 3 != 0)
            {
                throw new ArgumentException("Indices length must be a multiple of 3 (3 indices per triangle).", nameof(indices));
            }

            Positions = positions;
            Normals = normals;
            Indices = indices;
        }
    }
}