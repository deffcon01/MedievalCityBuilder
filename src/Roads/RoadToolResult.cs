using System;

namespace MedievalCityBuilder.Roads
{
    /// <summary>
    /// Failure reasons for the road tool.
    /// </summary>
    public enum RoadToolFailure
    {
        /// <summary>
        /// No failure.
        /// </summary>
        None,

        /// <summary>
        /// The click landed on the same node as the pending start.
        /// </summary>
        SameNode,

        /// <summary>
        /// The placement failed terrain validation.
        /// </summary>
        Terrain,

        /// <summary>
        /// A segment between the two endpoints already exists.
        /// </summary>
        DuplicateSegment,

        /// <summary>
        /// The budget cannot afford the cost.
        /// </summary>
        InsufficientFunds
    }

    /// <summary>
    /// Result of a road tool click.
    /// </summary>
    public readonly record struct RoadToolResult(
        bool Success,
        RoadToolFailure Failure,
        RoadPlacementFailure TerrainFailure,
        RoadCost Cost,
        RoadNodeId? StartNode,
        RoadNodeId? EndNode,
        RoadSegmentId? Segment)
    {
        /// <summary>
        /// Creates a successful result.
        /// </summary>
        public static RoadToolResult Ok(
            RoadCost cost,
            RoadNodeId? startNode,
            RoadNodeId? endNode,
            RoadSegmentId? segment)
        {
            return new RoadToolResult(
                true,
                RoadToolFailure.None,
                RoadPlacementFailure.None,
                cost,
                startNode,
                endNode,
                segment);
        }

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        public static RoadToolResult Fail(
            RoadToolFailure failure,
            RoadPlacementFailure terrainFailure,
            RoadCost cost,
            RoadNodeId? startNode = null,
            RoadNodeId? endNode = null,
            RoadSegmentId? segment = null)
        {
            return new RoadToolResult(
                false,
                failure,
                terrainFailure,
                cost,
                startNode,
                endNode,
                segment);
        }
    }

    /// <summary>
    /// Preview of a road tool action.
    /// </summary>
    public readonly record struct RoadToolPreview(
        bool IsValid,
        RoadToolFailure Failure,
        RoadPlacementFailure TerrainFailure,
        RoadCost Cost,
        double Length,
        double SnappedX,
        double SnappedZ,
        bool HasPendingStart);
}