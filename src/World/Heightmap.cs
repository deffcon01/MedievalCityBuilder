using System;

namespace MedievalCityBuilder.World
{
    /// <summary>
    /// Represents a heightmap with normalized height values in [0, 1].
    /// Width and Height are in vertices (not cells).
    /// </summary>
    public sealed class Heightmap
    {
        private readonly float[] _data;
        private readonly int _width;
        private readonly int _height;

        /// <summary>
        /// Gets the width in vertices.
        /// </summary>
        public int Width => _width;

        /// <summary>
        /// Gets the height in vertices.
        /// </summary>
        public int Height => _height;

        /// <summary>
        /// Creates a new Heightmap with the specified dimensions.
        /// </summary>
        /// <param name="width">Width in vertices (must be >= 2).</param>
        /// <param name="height">Height in vertices (must be >= 2).</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when width or height is less than 2.</exception>
        public Heightmap(int width, int height)
        {
            if (width < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be >= 2.");
            }
            if (height < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Height must be >= 2.");
            }

            _width = width;
            _height = height;
            _data = new float[width * height];
        }

        /// <summary>
        /// Gets the height value at the specified coordinates.
        /// </summary>
        /// <param name="x">X coordinate in vertices [0, Width-1].</param>
        /// <param name="z">Z coordinate in vertices [0, Height-1].</param>
        /// <returns>Normalized height value in [0, 1].</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when x or z is out of range.</exception>
        public float Get(int x, int z)
        {
            if (x < 0 || x >= _width)
            {
                throw new ArgumentOutOfRangeException(nameof(x), $"X coordinate {x} is out of range [0, {_width - 1}].");
            }
            if (z < 0 || z >= _height)
            {
                throw new ArgumentOutOfRangeException(nameof(z), $"Z coordinate {z} is out of range [0, {_height - 1}].");
            }

            return _data[z * _width + x];
        }

        /// <summary>
        /// Sets the height value at the specified coordinates.
        /// </summary>
        /// <param name="x">X coordinate in vertices [0, Width-1].</param>
        /// <param name="z">Z coordinate in vertices [0, Height-1].</param>
        /// <param name="value">Normalized height value in [0, 1].</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when x or z is out of range.</exception>
        public void Set(int x, int z, float value)
        {
            if (x < 0 || x >= _width)
            {
                throw new ArgumentOutOfRangeException(nameof(x), $"X coordinate {x} is out of range [0, {_width - 1}].");
            }
            if (z < 0 || z >= _height)
            {
                throw new ArgumentOutOfRangeException(nameof(z), $"Z coordinate {z} is out of range [0, {_height - 1}].");
            }

            _data[z * _width + x] = value;
        }

        /// <summary>
        /// Gets the raw height data as a read-only span.
        /// </summary>
        /// <returns>ReadOnlySpan of the height data in row-major order.</returns>
        public ReadOnlySpan<float> GetData()
        {
            return _data;
        }

        /// <summary>
        /// Gets the raw height data as a span for writing.
        /// </summary>
        /// <returns>Span of the height data in row-major order.</returns>
        public Span<float> GetDataSpan()
        {
            return _data;
        }
    }
}