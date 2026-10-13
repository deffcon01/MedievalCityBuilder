namespace MedievalCityBuilder.Core;

/// <summary>
/// Pure C# camera math helpers. No Godot types, no scene coupling.
/// Used by the presentation CameraRig for all clamping/wrapping.
/// </summary>
public static class CameraMath
{
    /// <summary>
    /// Clamps a zoom value into [min, max].
    /// </summary>
    public static float ClampZoom(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    /// <summary>
    /// Clamps (x, z) into the axis-aligned bounds defined by minX, maxX, minZ, maxZ.
    /// Returns the clamped pair as (x, z).
    /// </summary>
    public static (float x, float z) ClampToBounds(float x, float z, float minX, float maxX, float minZ, float maxZ)
    {
        if (x < minX) x = minX;
        if (x > maxX) x = maxX;
        if (z < minZ) z = minZ;
        if (z > maxZ) z = maxZ;
        return (x, z);
    }

    /// <summary>
    /// Wraps a yaw angle in degrees into the range (-180, 180].
    /// </summary>
    public static float WrapYaw(float degrees)
    {
        float wrapped = degrees % 360f;
        if (wrapped > 180f) wrapped -= 360f;
        if (wrapped <= -180f) wrapped += 360f;
        return wrapped;
    }
}