using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;

using MedievalCityBuilder.Roads;
using MedievalCityBuilder.World;

namespace MedievalCityBuilder.Presentation;

/// <summary>
/// Road tool presentation node. Handles input, terrain picking, markers, and simple line display.
/// All road placement rules live in src/Roads; this node only handles presentation.
/// The road graph is not saved yet, so roads are lost on F7, N, and restart.
/// The road ribbon mesh is a separate task (1.3c-iv).
/// </summary>
public partial class RoadTool : Node3D
{
    private TerrainView? _terrain;
    private RoadGraph? _graph;
    private RoadPlacementController? _controller;
    private MeshInstance3D? _ghost;
    private StandardMaterial3D? _ghostMaterial;
    private CanvasLayer? _labelLayer;
    private Label? _label;
    private Node3D? _visuals;
    private Dictionary<int, MeshInstance3D> _nodeMarkers = new();
    private MeshInstance3D? _pendingMarker;
    private StandardMaterial3D? _nodeMarkerMaterial;
    private StandardMaterial3D? _roadLineMaterial;
    private string _lastMessage = "";
    private bool _active = false;

    public bool Active => _active;

    /// <summary>
    /// Initializes the road tool with the given terrain view.
    /// </summary>
    public void Initialize(TerrainView terrain)
    {
        _terrain = terrain ?? throw new ArgumentNullException(nameof(terrain));
        _terrain.WorldApplied += OnWorldApplied;

        BuildGhost();
        BuildLabel();
        BuildVisuals();

        ResetForWorld();

        GD.Print("RoadTool: ready (R toggles road tool)");
    }

    private void OnWorldApplied()
    {
        ResetForWorld();
    }

    private void BuildVisuals()
    {
        _visuals = new Node3D
        {
            Name = "RoadVisuals",
            TopLevel = true
        };
        AddChild(_visuals);

        // Shared materials
        _nodeMarkerMaterial = new StandardMaterial3D
        {
            ShadingMode = StandardMaterial3D.ShadingModeEnum.Unshaded,
            AlbedoColor = new Color(0.95f, 0.5f, 0.1f, 1.0f) // Orange
        };

        _roadLineMaterial = new StandardMaterial3D
        {
            ShadingMode = StandardMaterial3D.ShadingModeEnum.Unshaded,
            AlbedoColor = new Color(0.35f, 0.2f, 0.1f, 1.0f) // Dark brown
        };

        // Pending start marker (yellow unshaded alpha sphere, radius 0.5)
        var pendingMesh = new SphereMesh { Radius = 0.5f };
        var pendingMat = new StandardMaterial3D
        {
            ShadingMode = StandardMaterial3D.ShadingModeEnum.Unshaded,
            Transparency = StandardMaterial3D.TransparencyEnum.Alpha,
            AlbedoColor = new Color(1.0f, 0.85f, 0.1f, 0.9f) // Yellow with alpha
        };
        _pendingMarker = new MeshInstance3D
        {
            Mesh = pendingMesh,
            Name = "PendingMarker",
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };
        _pendingMarker.SetSurfaceOverrideMaterial(0, pendingMat);
        _pendingMarker.Hide();
        _visuals.AddChild(_pendingMarker);
    }

    private void BuildGhost()
    {
        var sphereMesh = new SphereMesh
        {
            Radius = 0.4f,
            Height = 0.8f
        };

        _ghostMaterial = new StandardMaterial3D
        {
            ShadingMode = StandardMaterial3D.ShadingModeEnum.Unshaded,
            Transparency = StandardMaterial3D.TransparencyEnum.Alpha,
            AlbedoColor = new Color(0.2f, 0.9f, 0.2f, 0.8f) // Valid color (green)
        };

        _ghost = new MeshInstance3D
        {
            Mesh = sphereMesh,
            Name = "RoadGhost",
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };
        _ghost.SetSurfaceOverrideMaterial(0, _ghostMaterial);
        _ghost.Hide();

        AddChild(_ghost);
    }

    private void BuildLabel()
    {
        _label = new Label
        {
            Position = new Vector2(12, 12),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Text = "",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };

        _labelLayer = new CanvasLayer
        {
            Layer = 10
        };
        _labelLayer.AddChild(_label);
        AddChild(_labelLayer);
        _labelLayer.Hide();
    }

    private void ResetForWorld()
    {
        if (_terrain == null) return;

        // Free all children of _visuals except _pendingMarker
        if (_visuals != null)
        {
            var children = new Godot.Collections.Array<Node>(_visuals.GetChildren());
            foreach (var child in children)
            {
                if (child == _pendingMarker)
                {
                    continue;
                }
                _visuals.RemoveChild(child);
                child.QueueFree();
            }
        }

        _nodeMarkers.Clear();
        _lastMessage = "";

        _graph = new RoadGraph();
        // Budget is null because stockpiles do not exist yet; roads are free for now.
        _controller = new RoadPlacementController(
            _graph,
            _terrain.Surface,
            _terrain.Water,
            RoadPlacementSettings.Default,
            RoadCostSettings.Default,
            RoadToolSettings.Default,
            null);

        if (_ghost != null)
        {
            _ghost.Hide();
        }
        if (_pendingMarker != null)
        {
            _pendingMarker.Hide();
        }
        if (_label != null)
        {
            _label.Hide();
            _label.Text = "";
        }
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo && keyEvent.Keycode == Key.R)
        {
            _active = !_active;

            if (!_active)
            {
                // Tool turning OFF
                _controller?.Cancel();
                if (_ghost != null) _ghost.Hide();
                if (_pendingMarker != null) _pendingMarker.Hide();
                _lastMessage = "";
                if (_label != null) _label.Text = "";
            }

            if (_ghost != null) _ghost.Visible = _active;
            if (_labelLayer != null) _labelLayer.Visible = _active;

            GetViewport().SetInputAsHandled();
        }
    }

    private bool TryPick(out TerrainRayHit hit)
    {
        hit = default;
        var camera = GetViewport().GetCamera3D();
        if (camera == null || _terrain == null)
        {
            return false;
        }

        var mouse = GetViewport().GetMousePosition();
        var origin = camera.ProjectRayOrigin(mouse);
        var dir = camera.ProjectRayNormal(mouse);

        // Step 0.5 is half the default cell size, as the raycaster requires.
        return TerrainRaycaster.TryRaycast(
            _terrain.Surface,
            (double)origin.X, (double)origin.Y, (double)origin.Z,
            (double)dir.X, (double)dir.Y, (double)dir.Z,
            300.0, 0.5,
            out hit);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!_active || @event is not InputEventMouseButton mouseEvent || !mouseEvent.Pressed)
        {
            return;
        }

        if (mouseEvent.ButtonIndex == MouseButton.Left)
        {
            if (!TryPick(out var hit))
            {
                _lastMessage = "No terrain under cursor";
                GetViewport().SetInputAsHandled();
                return;
            }

            var result = _controller!.Click(hit.X, hit.Z);

            if (result.Success)
            {
                if (!result.Segment.HasValue)
                {
                    _lastMessage = "Start set";
                }
                else
                {
                    AddSegmentVisuals(result);
                    _lastMessage = string.Format(CultureInfo.InvariantCulture,
                        "Placed: cost {0} gold, {1} stone", result.Cost.Gold, result.Cost.Stone);
                }
            }
            else
            {
                string failureText;
                if (result.Failure == RoadToolFailure.Terrain)
                {
                    failureText = result.TerrainFailure.ToString();
                }
                else
                {
                    failureText = result.Failure.ToString();
                }
                _lastMessage = "Failed: " + failureText;
            }

            GetViewport().SetInputAsHandled();
        }
        else if (mouseEvent.ButtonIndex == MouseButton.Right)
        {
            _controller!.Cancel();
            _lastMessage = "Cancelled";
            if (_pendingMarker != null) _pendingMarker.Hide();
            GetViewport().SetInputAsHandled();
        }
        // Other buttons (middle, wheel) are NOT handled so camera zoom still works.
    }

    private void AddSegmentVisuals(RoadToolResult result)
    {
        if (_graph == null || _terrain == null || _visuals == null || _nodeMarkerMaterial == null || _roadLineMaterial == null)
        {
            return;
        }

        if (!result.StartNode.HasValue || !result.EndNode.HasValue || !result.Segment.HasValue)
        {
            return;
        }

        // Add node markers for start and end
        foreach (var nodeId in new[] { result.StartNode.Value, result.EndNode.Value })
        {
            if (!_nodeMarkers.ContainsKey(nodeId.Value))
            {
                var node = _graph.GetNode(nodeId);
                double y = _terrain.Surface.SampleHeight(node.X, node.Z) + 0.35;

                var markerMesh = new SphereMesh { Radius = 0.35f };
                var marker = new MeshInstance3D
                {
                    Mesh = markerMesh,
                    Name = $"NodeMarker_{nodeId.Value}",
                    GlobalPosition = new Vector3((float)node.X, (float)y, (float)node.Z),
                    CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
                };
                marker.SetSurfaceOverrideMaterial(0, _nodeMarkerMaterial);
                _visuals.AddChild(marker);
                _nodeMarkers[nodeId.Value] = marker;
            }
        }

        // Add road line
        var segment = _graph.GetSegment(result.Segment.Value);
        var nodeA = _graph.GetNode(segment.A);
        var nodeB = _graph.GetNode(segment.B);

        var points = RoadPolyline.Sample(_terrain.Surface, nodeA.X, nodeA.Z, nodeB.X, nodeB.Z, 1.0, 0.15);

        var vertices = new Vector3[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            vertices[i] = new Vector3((float)points[i].X, (float)points[i].Y, (float)points[i].Z);
        }

        var arrayMesh = new ArrayMesh();
        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = vertices;
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.LineStrip, arrays);

        var lineInstance = new MeshInstance3D
        {
            Mesh = arrayMesh,
            Name = $"RoadLine_{result.Segment.Value.Value}",
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };
        lineInstance.SetSurfaceOverrideMaterial(0, _roadLineMaterial);
        _visuals.AddChild(lineInstance);

        // Lines are 1 pixel wide; a proper road ribbon mesh is a later task (1.3c-iv).
    }

    public override void _Process(double delta)
    {
        if (!_active || _terrain == null || _controller == null || _ghost == null || _ghostMaterial == null || _label == null)
        {
            return;
        }

        bool hasHit = TryPick(out var hit);
        RoadToolPreview? preview = null;

        if (hasHit && _terrain.Surface.Contains(hit.X, hit.Z))
        {
            preview = _controller.Preview(hit.X, hit.Z);
        }

        // Update pending marker
        if (_pendingMarker != null && _controller.HasPendingStart)
        {
            double y = _terrain.Surface.SampleHeight(_controller.PendingStartX, _controller.PendingStartZ) + 0.5;
            _pendingMarker.GlobalPosition = new Vector3((float)_controller.PendingStartX, (float)y, (float)_controller.PendingStartZ);
            _pendingMarker.Show();
        }
        else if (_pendingMarker != null)
        {
            _pendingMarker.Hide();
        }

        // Build label lines
        var lines = new List<string>();

        // Line 1
        lines.Add("Road tool: ON (R toggle, LMB place, RMB cancel)");

        // Line 2
        string line2;
        if (hasHit && preview.HasValue && _terrain.Surface.Contains(preview.Value.SnappedX, preview.Value.SnappedZ))
        {
            double y = _terrain.Surface.SampleHeight(preview.Value.SnappedX, preview.Value.SnappedZ) + 0.6;
            _ghost.GlobalPosition = new Vector3((float)preview.Value.SnappedX, (float)y, (float)preview.Value.SnappedZ);
            _ghost.Show();

            if (preview.Value.IsValid)
            {
                _ghostMaterial.AlbedoColor = new Color(0.2f, 0.9f, 0.2f, 0.8f); // Green
                line2 = string.Format(CultureInfo.InvariantCulture, "Point OK at ({0:F1}, {1:F1})", preview.Value.SnappedX, preview.Value.SnappedZ);
            }
            else
            {
                _ghostMaterial.AlbedoColor = new Color(0.95f, 0.2f, 0.2f, 0.8f); // Red
                string failureText;
                if (preview.Value.Failure == RoadToolFailure.Terrain)
                {
                    failureText = preview.Value.TerrainFailure.ToString();
                }
                else
                {
                    failureText = preview.Value.Failure.ToString();
                }
                line2 = "Invalid: " + failureText;
            }
        }
        else
        {
            _ghost.Hide();
            line2 = "No terrain under cursor";
        }
        lines.Add(line2);

        // Line 3: Segment preview info (only when HasPendingStart and preview is valid)
        if (hasHit && preview.HasValue && preview.Value.HasPendingStart && preview.Value.IsValid)
        {
            string line3 = string.Format(CultureInfo.InvariantCulture,
                "Segment: {0:F1} units, cost {1} gold, {2} stone",
                preview.Value.Length, preview.Value.Cost.Gold, preview.Value.Cost.Stone);
            lines.Add(line3);
        }

        // Line 4
        if (_graph != null)
        {
            lines.Add($"Roads: {_graph.SegmentCount} segments, {_graph.NodeCount} nodes");
        }

        // Line 5
        if (!string.IsNullOrEmpty(_lastMessage))
        {
            lines.Add(_lastMessage);
        }

        _label.Text = string.Join("\n", lines);
    }
}