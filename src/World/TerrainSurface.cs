using System;

namespace MedievalCityBuilder.World
{
    /// <summary>
    /// Describes how world coordinates map onto a Heightmap.
    /// This mirrors TerrainMeshData's vertex layout: vertex (i,j) is at origin + (i*cellSize, j*cellSize).
    /// </summary>
    public sealed class TerrainSurface
    {
        private readonly Heightmap _heightmap;
        private readonly double _cellSize;
        private readonly double _heightScale;
        private readonly double _originX;
        private readonly double _originZ;

        /// <summary>
        /// Creates a new TerrainSurface.
        /// </summary>
        /// <param name="heightmap">The heightmap to sample from.</param>
        /// <param name="cellSize">World size of one grid cell (must be > 0).</param>
        /// <param name="heightScale">Scale factor from normalized [0,1] to world height (must be >= 0).</param>
        /// <param name="originX">World X coordinate of heightmap vertex (0,0).</param>
        /// <param name="originZ">World Z coordinate of heightmap vertex (0,0).</param>
        /// <exception cref="ArgumentNullException">Thrown when heightmap is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when cellSize <= 0, heightScale < 0, or originX/originZ is NaN or infinite.</exception>
        public TerrainSurface(Heightmap heightmap, double cellSize, double heightScale, double originX, double originZ)
        {
            if (heightmap == null)
            {
                throw new ArgumentNullException(nameof(heightmap), "Heightmap must not be null.");
            }
            if (cellSize <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(cellSize), "CellSize must be > 0.");
            }
            if (heightScale < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(heightScale), "HeightScale must be >= 0.");
            }
            if (double.IsNaN(originX) || double.IsInfinity(originX))
            {
                throw new ArgumentOutOfRangeException(nameof(originX), "OriginX must be a finite number.");
            }
            if (double.IsNaN(originZ) || double.IsInfinity(originZ))
            {
                throw new ArgumentOutOfRangeException(nameof(originZ), "OriginZ must be a finite number.");
            }

            _heightmap = heightmap;
            _cellSize = cellSize;
            _heightScale = heightScale;
            _originX = originX;
            _originZ = originZ;
        }

        /// <summary>
        /// Creates a TerrainSurface centered at world origin (0,0).
        /// The origin is set to (-(Width-1)*cellSize/2, -(Height-1)*cellSize/2),
        /// matching how TerrainView centers the terrain.
        /// </summary>
        /// <param name="heightmap">The heightmap to sample from.</param>
        /// <param name="cellSize">World size of one grid cell (must be > 0).</param>
        /// <param name="heightScale">Scale factor from normalized [0,1] to world height (must be >= 0).</param>
        /// <returns>A new TerrainSurface centered at world origin.</returns>
        public static TerrainSurface Centered(Heightmap heightmap, double cellSize, double heightScale)
        {
            double originX = -((heightmap.Width - 1) * cellSize) / 2.0;
            double originZ = -((heightmap.Height - 1) * cellSize) / 2.0;
            return new TerrainSurface(heightmap, cellSize, heightScale, originX, originZ);
        }

        /// <summary>
        /// Minimum world X coordinate (inclusive).
        /// </summary>
        public double MinX => _originX;

        /// <summary>
        /// Maximum world X coordinate (inclusive).
        /// </summary>
        public double MaxX => _originX + (_heightmap.Width - 1) * _cellSize;

        /// <summary>
        /// Minimum world Z coordinate (inclusive).
        /// </summary>
        public double MinZ => _originZ;

        /// <summary>
        /// Maximum world Z coordinate (inclusive).
        /// </summary>
        public double MaxZ => _originZ + (_heightmap.Height - 1) * _cellSize;

        /// <summary>
        /// Checks if a world coordinate lies within the terrain bounds (inclusive of all four edges).
        /// </summary>
        /// <param name="x">World X coordinate.</param>
        /// <param name="z">World Z coordinate.</param>
        /// <returns>True if the point is within bounds, false otherwise. Returns false for NaN coordinates.</returns>
        public bool Contains(double x, double z)
        {
            if (double.IsNaN(x) || double.IsNaN(z))
            {
                return false;
            }

            return x >= MinX && x <= MaxX && z >= MinZ && z <= MaxZ;
        }

        /// <summary>
        /// Samples the normalized height at a world coordinate using bilinear interpolation.
        /// </summary>
        /// <param name="x">World X coordinate.</param>
        /// <param name="z">World Z coordinate.</param>
        /// <returns>Normalized height in [0, 1].</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the point is outside the terrain bounds or coordinates are NaN.</exception>
        public double SampleNormalized(double x, double z)
        {
            if (double.IsNaN(x) || double.IsNaN(z))
            {
                throw new ArgumentOutOfRangeException(nameof(x), "Coordinates must not be NaN.");
            }

            if (!Contains(x, z))
            {
                throw new ArgumentOutOfRangeException(nameof(x), "Point is outside terrain bounds.");
            }

            // Convert world coordinates to heightmap UV coordinates
            double u = (x - _originX) / _cellSize;
            double v = (z - _originZ) / _cellSize;

            // Clamp to valid vertex indices [0, Width-1] and [0, Height-1]
            // This ensures the upper edge works correctly (x == MaxX samples vertex Width-1 exactly)
            int x0 = (int)Math.Floor(u);
            int z0 = (int)Math.Floor(v);

            // Clamp to the last vertex for the upper edge
            if (x0 >= _heightmap.Width - 1)
            {
                x0 = _heightmap.Width - 2;
            }
            if (z0 >= _heightmap.Height - 1)
            {
                z0 = _heightmap.Height - 2;
            }

            int x1 = x0 + 1;
            int z1 = z0 + 1;

            // Fractional parts for interpolation
            double fx = u - x0;
            double fz = v - z0;

            // Get the four corner values
            double h00 = _heightmap.Get(x0, z0);
            double h10 = _heightmap.Get(x1, z0);
            double h01 = _heightmap.Get(x0, z1);
            double h11 = _heightmap.Get(x1, z1);

            // Bilinear interpolation
            double h0 = h00 * (1.0 - fx) + h10 * fx;
            double h1 = h01 * (1.0 - fx) + h11 * fx;
            double result = h0 * (1.0 - fz) + h1 * fz;

            return result;
        }

        /// <summary>
        /// Samples the world height at a world coordinate.
        /// </summary>
        /// <param name="x">World X coordinate.</param>
        /// <param name="z">World Z coordinate.</param>
        /// <returns>World height (SampleNormalized * heightScale).</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the point is outside the terrain bounds or coordinates are NaN.</exception>
        public double SampleHeight(double x, double z)
        {
            return SampleNormalized(x, z) * _heightScale;
        }
    }
}