using System;
using MedievalCityBuilder.World;

namespace MedievalCityBuilder.Roads
{
    /// <summary>
    /// Controller for click-to-place road drawing logic.
    /// Scene input, ghost meshes and road meshes are separate tasks; splitting an existing segment
    /// when a click lands in its middle is deferred; saving the road graph is deferred.
    /// </summary>
    public sealed class RoadPlacementController
    {
        private readonly RoadGraph _graph;
        private readonly TerrainSurface _surface;
        private readonly WaterSettings _water;
        private readonly RoadPlacementSettings _placement;
        private readonly RoadCostSettings _cost;
        private readonly RoadToolSettings _tool;
        private readonly IRoadBudget? _budget;

        private bool _hasPendingStart;
        private double _pendingStartX;
        private double _pendingStartZ;
        private RoadNodeId? _pendingStartNode;

        /// <summary>
        /// Creates a new RoadPlacementController.
        /// </summary>
        public RoadPlacementController(
            RoadGraph graph,
            TerrainSurface surface,
            WaterSettings water,
            RoadPlacementSettings placement,
            RoadCostSettings cost,
            RoadToolSettings tool,
            IRoadBudget? budget = null)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _surface = surface ?? throw new ArgumentNullException(nameof(surface));
            _water = water ?? throw new ArgumentNullException(nameof(water));
            _placement = placement ?? throw new ArgumentNullException(nameof(placement));
            _cost = cost ?? throw new ArgumentNullException(nameof(cost));
            _tool = tool ?? throw new ArgumentNullException(nameof(tool));
            _budget = budget;
        }

        /// <summary>
        /// Whether there is a pending start node or free point.
        /// </summary>
        public bool HasPendingStart => _hasPendingStart;

        /// <summary>
        /// The X coordinate of the pending start point.
        /// </summary>
        public double PendingStartX
        {
            get
            {
                if (!_hasPendingStart)
                {
                    throw new InvalidOperationException("There is no pending start.");
                }
                return _pendingStartX;
            }
        }

        /// <summary>
        /// The Z coordinate of the pending start point.
        /// </summary>
        public double PendingStartZ
        {
            get
            {
                if (!_hasPendingStart)
                {
                    throw new InvalidOperationException("There is no pending start.");
                }
                return _pendingStartZ;
            }
        }

        /// <summary>
        /// The node ID of the pending start point, or null if it is a free point not yet in the graph.
        /// If the stored node was removed from the graph (e.g. by demolition), returns null even though
        /// HasPendingStart is still true; the pending start then behaves as a free point at the
        /// stored PendingStartX / PendingStartZ coordinates.
        /// </summary>
        public RoadNodeId? PendingStartNode => GetLivePendingStartNode();

        /// <summary>
        /// Returns the stored pending start node only when a pending start exists and that node is
        /// still present in the graph; otherwise null. Never mutates state.
        /// </summary>
        private RoadNodeId? GetLivePendingStartNode()
        {
            if (!_hasPendingStart || !_pendingStartNode.HasValue)
            {
                return null;
            }

            if (!_graph.TryGetNode(_pendingStartNode.Value, out _))
            {
                return null;
            }

            return _pendingStartNode;
        }

        /// <summary>
        /// Clears the pending start point without modifying the road graph.
        /// Calling Cancel with no pending start does nothing and does not throw.
        /// </summary>
        public void Cancel()
        {
            _hasPendingStart = false;
            _pendingStartX = 0.0;
            _pendingStartZ = 0.0;
            _pendingStartNode = null;
        }

        private (double X, double Z, RoadNodeId? NodeId) FindSnappedPoint(double x, double z)
        {
            if (double.IsNaN(x) || double.IsInfinity(x) || double.IsNaN(z) || double.IsInfinity(z))
            {
                return (x, z, null);
            }

            RoadNode? bestNode = null;
            double bestDistSq = _tool.SnapRadius * _tool.SnapRadius;

            // Linear scan over graph.Nodes (spatial index is a later optimization)
            var nodes = _graph.Nodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                double dx = node.X - x;
                double dz = node.Z - z;
                double distSq = dx * dx + dz * dz;

                if (distSq <= bestDistSq)
                {
                    if (bestNode == null || distSq < bestDistSq || (distSq == bestDistSq && node.Id.Value < bestNode.Id.Value))
                    {
                        bestNode = node;
                        bestDistSq = distSq;
                    }
                }
            }

            if (bestNode != null)
            {
                return (bestNode.X, bestNode.Z, bestNode.Id);
            }

            return (x, z, null);
        }

        /// <summary>
        /// Previews placing a road endpoint at (x, z) without modifying state or spending funds.
        /// </summary>
        public RoadToolPreview Preview(double x, double z)
        {
            var snapped = FindSnappedPoint(x, z);

            if (!_hasPendingStart)
            {
                var nodeValidation = RoadPlacementValidator.ValidateNode(_surface, _water, snapped.X, snapped.Z);
                if (!nodeValidation.IsValid)
                {
                    return new RoadToolPreview(
                        false,
                        RoadToolFailure.Terrain,
                        nodeValidation.Failure,
                        RoadCost.Zero,
                        0.0,
                        x,
                        z,
                        false);
                }

                return new RoadToolPreview(
                    true,
                    RoadToolFailure.None,
                    RoadPlacementFailure.None,
                    RoadCost.Zero,
                    0.0,
                    snapped.X,
                    snapped.Z,
                    false);
            }

            // With pending start
            RoadNodeId? liveStart = GetLivePendingStartNode();
            if (snapped.NodeId.HasValue && liveStart.HasValue && snapped.NodeId.Value.Value == liveStart.Value.Value)
            {
                return new RoadToolPreview(
                    false,
                    RoadToolFailure.SameNode,
                    RoadPlacementFailure.None,
                    RoadCost.Zero,
                    0.0,
                    snapped.X,
                    snapped.Z,
                    true);
            }

            var segmentValidation = RoadPlacementValidator.ValidateSegment(
                _surface,
                _water,
                _placement,
                _pendingStartX,
                _pendingStartZ,
                snapped.X,
                snapped.Z);

            if (!segmentValidation.IsValid)
            {
                return new RoadToolPreview(
                    false,
                    RoadToolFailure.Terrain,
                    segmentValidation.Failure,
                    RoadCost.Zero,
                    0.0,
                    snapped.X,
                    snapped.Z,
                    true);
            }

            if (liveStart.HasValue && snapped.NodeId.HasValue)
            {
                if (DoesSegmentExist(liveStart.Value, snapped.NodeId.Value))
                {
                    return new RoadToolPreview(
                        false,
                        RoadToolFailure.DuplicateSegment,
                        RoadPlacementFailure.None,
                        RoadCost.Zero,
                        0.0,
                        snapped.X,
                        snapped.Z,
                        true);
                }
            }

            double dx = snapped.X - _pendingStartX;
            double dz = snapped.Z - _pendingStartZ;
            double length = Math.Sqrt(dx * dx + dz * dz);
            RoadCost cost = RoadCostCalculator.GetSegmentCost(_cost, length);

            if (_budget != null && !_budget.CanAfford(cost))
            {
                return new RoadToolPreview(
                    false,
                    RoadToolFailure.InsufficientFunds,
                    RoadPlacementFailure.None,
                    cost,
                    length,
                    snapped.X,
                    snapped.Z,
                    true);
            }

            return new RoadToolPreview(
                true,
                RoadToolFailure.None,
                RoadPlacementFailure.None,
                cost,
                length,
                snapped.X,
                snapped.Z,
                true);
        }

        private bool DoesSegmentExist(RoadNodeId a, RoadNodeId b)
        {
            var segments = _graph.GetSegmentsAt(a);
            for (int i = 0; i < segments.Count; i++)
            {
                var seg = segments[i];
                if ((seg.A.Value == a.Value && seg.B.Value == b.Value) || (seg.A.Value == b.Value && seg.B.Value == a.Value))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Handles a click at world coordinates (x, z).
        /// </summary>
        public RoadToolResult Click(double x, double z)
        {
            var snapped = FindSnappedPoint(x, z);

            if (!_hasPendingStart)
            {
                var nodeValidation = RoadPlacementValidator.ValidateNode(_surface, _water, snapped.X, snapped.Z);
                if (!nodeValidation.IsValid)
                {
                    return RoadToolResult.Fail(RoadToolFailure.Terrain, nodeValidation.Failure, RoadCost.Zero);
                }

                _hasPendingStart = true;
                _pendingStartX = snapped.X;
                _pendingStartZ = snapped.Z;
                _pendingStartNode = snapped.NodeId;

                return RoadToolResult.Ok(RoadCost.Zero, snapped.NodeId, null, null);
            }

            // WITH PENDING START
            RoadNodeId? liveStart = GetLivePendingStartNode();
            if (snapped.NodeId.HasValue && liveStart.HasValue && snapped.NodeId.Value.Value == liveStart.Value.Value)
            {
                return RoadToolResult.Fail(RoadToolFailure.SameNode, RoadPlacementFailure.None, RoadCost.Zero);
            }

            var segmentValidation = RoadPlacementValidator.ValidateSegment(
                _surface,
                _water,
                _placement,
                _pendingStartX,
                _pendingStartZ,
                snapped.X,
                snapped.Z);

            if (!segmentValidation.IsValid)
            {
                return RoadToolResult.Fail(RoadToolFailure.Terrain, segmentValidation.Failure, RoadCost.Zero);
            }

            if (liveStart.HasValue && snapped.NodeId.HasValue)
            {
                if (DoesSegmentExist(liveStart.Value, snapped.NodeId.Value))
                {
                    return RoadToolResult.Fail(RoadToolFailure.DuplicateSegment, RoadPlacementFailure.None, RoadCost.Zero);
                }
            }

            double dx = snapped.X - _pendingStartX;
            double dz = snapped.Z - _pendingStartZ;
            double length = Math.Sqrt(dx * dx + dz * dz);
            RoadCost cost = RoadCostCalculator.GetSegmentCost(_cost, length);

            if (_budget != null && !_budget.TrySpend(cost))
            {
                return RoadToolResult.Fail(RoadToolFailure.InsufficientFunds, RoadPlacementFailure.None, cost);
            }

            // All validation happens before payment and before any mutation, so a failure never leaves a partial state.
            RoadNodeId startNodeId = liveStart ?? _graph.AddNode(_pendingStartX, _pendingStartZ);
            RoadNodeId endNodeId = snapped.NodeId ?? _graph.AddNode(snapped.X, snapped.Z);
            RoadSegmentId segmentId = _graph.AddSegment(startNodeId, endNodeId);

            if (_tool.ContinueChain)
            {
                _hasPendingStart = true;
                _pendingStartX = snapped.X;
                _pendingStartZ = snapped.Z;
                _pendingStartNode = endNodeId;
            }
            else
            {
                Cancel();
            }

            return RoadToolResult.Ok(cost, startNodeId, endNodeId, segmentId);
        }
    }
}
