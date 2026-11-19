using System;

namespace MedievalCityBuilder.Presentation
{
    /// <summary>
    /// Which editing tool is currently active (none, road, zone).
    /// Only one tool may be active at a time.
    /// </summary>
    public enum ToolMode
    {
        None,
        Road,
        Zone
    }
}