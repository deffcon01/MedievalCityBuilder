using System;

namespace MedievalCityBuilder.Zoning
{
    /// <summary>
    /// A pure, deterministic zone overlay mesh for display only.
    /// Each painted cell becomes one quad (4 vertices, 2 triangles) hugging the terrain.
    /// </summary>
    public sealed class ZoneOverlayMesh
    {
        /// <summary>
        /// Vertex positions in world coordinates. Layout: [x0, y0, z0, x1, y1, z1, ...].
        /// </summary>
        public float[] Positions { get; }

        /// <summary>
        /// One zone id per vertex (ZoneTypeId.Value). Cells with no zone are never emitted.
        /// </summary>
        public int[] VertexZoneIds { get; }

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
        /// Number of quads emitted (one per painted cell).
        /// </summary>
        public int QuadCount { get; }

        /// <summary>
        /// Creates a new ZoneOverlayMesh.
        /// </summary>
        /// <param name="positions">Vertex positions (x, y, z per vertex). Must not be null, length divisible by 3.</param>
        /// <param name="vertexZoneIds">One zone id per vertex. Must not be null, length must equal Positions.Length / 3.</param>
        /// <param name="indices">Triangle indices (3 per triangle). Must not be null, length divisible by 3.</param>
        /// <param name="quadCount">Number of quads emitted.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="ArgumentException">Thrown when lengths are inconsistent.</exception>
        public ZoneOverlayMesh(float[] positions, int[] vertexZoneIds, int[] indices, int quadCount)
        {
            if (positions == null)
            {
                throw new ArgumentNullException(nameof(positions), "Positions must not be null.");
            }
            if (vertexZoneIds == null)
            {
                throw new ArgumentNullException(nameof(vertexZoneIds), "VertexZoneIds must not be null.");
            }
            if (indices == null)
            {
                throw new ArgumentNullException(nameof(indices), "Indices must not be null.");
            }

            if (quadCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quadCount), "QuadCount must be >= 0.");
            }

            if (positions.Length / 3 != vertexZoneIds.Length)
            {
                throw new ArgumentException("Positions length must be 3 times VertexZoneIds length (x, y, z per vertex).", nameof(vertexZoneIds));
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
            VertexZoneIds = vertexZoneIds;
            Indices = indices;
            QuadCount = quadCount;
        }
    }
}