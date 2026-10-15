using System;

namespace MedievalCityBuilder.Roads
{
    /// <summary>
    /// Unique identifier for a road node.
    /// </summary>
    public readonly record struct RoadNodeId(int Value)
    {
        public override string ToString() => $"N{Value}";
    }

    /// <summary>
    /// Unique identifier for a road segment.
    /// </summary>
    public readonly record struct RoadSegmentId(int Value)
    {
        public override string ToString() => $"S{Value}";
    }
}
