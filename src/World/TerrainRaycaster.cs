using System;

namespace MedievalCityBuilder.World
{
    /// <summary>
    /// Performs raycasting against a <see cref="TerrainSurface"/>.
    /// Uses the same world coordinate system as TerrainSurface.
    /// Step should be at most half the cell size, because a larger step can skip thin ridges.
    /// A ray lying exactly on the surface reports a hit at its first in-bounds sample.
    /// Pure and deterministic.
    /// </summary>
    public static class TerrainRaycaster
    {
        /// <summary>
        /// Attempts to cast a ray against the specified terrain surface.
        /// </summary>
        /// <param name="surface">The terrain surface to test against.</param>
        /// <param name="originX">Ray origin X component.</param>
        /// <param name="originY">Ray origin Y component.</param>
        /// <param name="originZ">Ray origin Z component.</param>
        /// <param name="dirX">Ray direction X component.</param>
        /// <param name="dirY">Ray direction Y component.</param>
        /// <param name="dirZ">Ray direction Z component.</param>
        /// <param name="maxDistance">Maximum raycast distance along direction (must be > 0).</param>
        /// <param name="step">Ray marching step size along direction (must be > 0).</param>
        /// <param name="hit">Output hit details if a hit occurs, or default if no hit.</param>
        /// <returns>True if the ray intersects the terrain surface, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when surface is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when origin or direction component is NaN or infinite, or when maxDistance/step is invalid or ratio exceeds 1,000,000.</exception>
        /// <exception cref="ArgumentException">Thrown when direction vector length is less than 1e-12.</exception>
        public static bool TryRaycast(
            TerrainSurface surface,
            double originX, double originY, double originZ,
            double dirX, double dirY, double dirZ,
            double maxDistance, double step,
            out TerrainRayHit hit)
        {
            if (surface == null)
            {
                throw new ArgumentNullException(nameof(surface), "Surface must not be null.");
            }

            if (double.IsNaN(originX) || double.IsInfinity(originX) ||
                double.IsNaN(originY) || double.IsInfinity(originY) ||
                double.IsNaN(originZ) || double.IsInfinity(originZ))
            {
                throw new ArgumentOutOfRangeException(nameof(originX), "Origin components must be finite numbers.");
            }

            if (double.IsNaN(dirX) || double.IsInfinity(dirX) ||
                double.IsNaN(dirY) || double.IsInfinity(dirY) ||
                double.IsNaN(dirZ) || double.IsInfinity(dirZ))
            {
                throw new ArgumentOutOfRangeException(nameof(dirX), "Direction components must be finite numbers.");
            }

            double dirLen = Math.Sqrt(dirX * dirX + dirY * dirY + dirZ * dirZ);
            if (dirLen < 1e-12)
            {
                throw new ArgumentException("Direction vector length is too small.", nameof(dirX));
            }

            if (double.IsNaN(maxDistance) || double.IsInfinity(maxDistance) || maxDistance <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDistance), "MaxDistance must be a positive finite number.");
            }

            if (double.IsNaN(step) || double.IsInfinity(step) || step <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(step), "Step must be a positive finite number.");
            }

            if (maxDistance / step > 1000000.0)
            {
                // protects against huge loops
                throw new ArgumentOutOfRangeException(nameof(maxDistance), "MaxDistance / step ratio exceeds 1,000,000.");
            }

            double normDirX = dirX / dirLen;
            double normDirY = dirY / dirLen;
            double normDirZ = dirZ / dirLen;

            int steps = (int)Math.Ceiling(maxDistance / step);
            double prevT = 0.0;
            double prevF = 0.0;
            bool prevInBounds = false;

            for (int i = 0; i <= steps; i++)
            {
                double t = Math.Min(i * step, maxDistance);
                double px = originX + normDirX * t;
                double py = originY + normDirY * t;
                double pz = originZ + normDirZ * t;

                if (!surface.Contains(px, pz))
                {
                    prevInBounds = false;
                    continue;
                }

                double f = py - surface.SampleHeight(px, pz);
                if (f == 0.0)
                {
                    return CreateHit(surface, originX, originY, originZ, normDirX, normDirY, normDirZ, t, out hit);
                }

                if (f < 0.0)
                {
                    if (prevInBounds && prevF >= 0.0)
                    {
                        double lo = prevT;
                        double hi = t;
                        for (int iter = 0; iter < 50; iter++)
                        {
                            double mid = lo + (hi - lo) * 0.5;
                            double mpx = originX + normDirX * mid;
                            double mpy = originY + normDirY * mid;
                            double mpz = originZ + normDirZ * mid;
                            double fMid = mpy - surface.SampleHeight(mpx, mpz);
                            if (fMid >= 0.0)
                            {
                                lo = mid;
                            }
                            else
                            {
                                hi = mid;
                            }
                        }
                        double finalT = lo + (hi - lo) * 0.5;
                        return CreateHit(surface, originX, originY, originZ, normDirX, normDirY, normDirZ, finalT, out hit);
                    }
                    else
                    {
                        // a ray that starts or enters the terrain area below the surface reports no hit
                        hit = default;
                        return false;
                    }
                }

                // Otherwise (f > 0)
                prevT = t;
                prevF = f;
                prevInBounds = true;
            }

            hit = default;
            return false;
        }

        private static bool CreateHit(
            TerrainSurface surface,
            double originX, double originY, double originZ,
            double dirX, double dirY, double dirZ,
            double distance,
            out TerrainRayHit hit)
        {
            double rawX = originX + dirX * distance;
            double rawZ = originZ + dirZ * distance;

            double x = rawX;
            if (x < surface.MinX)
            {
                x = surface.MinX;
            }
            if (x > surface.MaxX)
            {
                x = surface.MaxX;
            }

            double z = rawZ;
            if (z < surface.MinZ)
            {
                z = surface.MinZ;
            }
            if (z > surface.MaxZ)
            {
                z = surface.MaxZ;
            }

            double y = surface.SampleHeight(x, z);
            hit = new TerrainRayHit(x, y, z, distance);
            return true;
        }
    }
}
