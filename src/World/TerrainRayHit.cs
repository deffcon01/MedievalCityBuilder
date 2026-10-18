using System;

namespace MedievalCityBuilder.World
{
    /// <summary>
    /// Represents the result of a terrain raycast hit.
    /// Uses the same world coordinate system as <see cref="TerrainSurface"/>.
    /// </summary>
    /// <param name="X">World X coordinate of the hit point (clamped to surface bounds).</param>
    /// <param name="Y">World Y coordinate of the hit point (sampled from surface).</param>
    /// <param name="Z">World Z coordinate of the hit point (clamped to surface bounds).</param>
    /// <param name="Distance">Distance along the ray from origin to hit point.</param>
    public readonly record struct TerrainRayHit(double X, double Y, double Z, double Distance);
}
