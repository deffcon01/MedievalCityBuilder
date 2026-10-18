using Godot;
using System;
using System.Globalization;

using MedievalCityBuilder.Roads;
using MedievalCityBuilder.World;

namespace MedievalCityBuilder.Presentation;

/// <summary>
/// Road tool presentation node. Handles input, terrain picking, and ghost preview.
/// All road placement rules live in src/Roads; this node only handles presentation.
/// Clicking, markers, and road meshes are separate tasks.
/// The road graph is not saved yet, so roads are lost on F7, N, and restart.
/// </summary>
public partial class RoadTool : Node3D
{
    private TerrainView? _terrain;
    private RoadPlacementController? _controller;
    private MeshInstance3D? _ghost;
    private StandardMaterial3D? _ghostMaterial;
    private CanvasLayer? _labelLayer;
    private Label? _label;
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

        ResetForWorld();

        GD.Print("RoadTool: ready (R toggles road tool)");
    }

    private void OnWorldApplied()
    {
        ResetForWorld();
    }

    private void ResetForWorld()
    {
        if (_terrain == null) return;

        var graph = new RoadGraph();
        // Budget is null because stockpiles do not exist yet; roads are free for now.
        _controller = new RoadPlacementController(
            graph,
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
        if (_label != null)
        {
            _label.Hide();
            _label.Text = "";
        }
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

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo && keyEvent.Keycode == Key.R)
        {
            _active = !_active;

            if (_ghost != null)
            {
                _ghost.Visible = _active;
            }
            if (_labelLayer != null)
            {
                _labelLayer.Visible = _active;
            }
            if (!_active && _label != null)
            {
                _label.Text = "";
            }

            GetViewport().SetInputAsHandled();
        }
    }

    public override void _Process(double delta)
    {
        if (!_active || _terrain == null || _controller == null || _ghost == null || _ghostMaterial == null || _label == null)
        {
            return;
        }

        var camera = GetViewport().GetCamera3D();
        if (camera == null)
        {
            _ghost.Hide();
            _label.Text = "Road tool: ON (R to toggle)\nNo terrain under cursor";
            return;
        }

        var mouse = GetViewport().GetMousePosition();
        var origin = camera.ProjectRayOrigin(mouse);
        var dir = camera.ProjectRayNormal(mouse);

        // Step 0.5 is half the default cell size, as the raycaster requires.
        if (TerrainRaycaster.TryRaycast(
                _terrain.Surface,
                origin.X, origin.Y, origin.Z,
                dir.X, dir.Y, dir.Z,
                300.0, 0.5,
                out var hit))
        {
            var preview = _controller.Preview(hit.X, hit.Z);

            if (_terrain.Surface.Contains(preview.SnappedX, preview.SnappedZ))
            {
                double y = _terrain.Surface.SampleHeight(preview.SnappedX, preview.SnappedZ) + 0.6;
                _ghost.GlobalPosition = new Vector3((float)preview.SnappedX, (float)y, (float)preview.SnappedZ);
                _ghost.Show();

                if (preview.IsValid)
                {
                    _ghostMaterial.AlbedoColor = new Color(0.2f, 0.9f, 0.2f, 0.8f); // Green
                }
                else
                {
                    _ghostMaterial.AlbedoColor = new Color(0.95f, 0.2f, 0.2f, 0.8f); // Red
                }

                string line2;
                if (preview.IsValid)
                {
                    line2 = string.Format(CultureInfo.InvariantCulture, "Point OK at ({0:F1}, {1:F1})", preview.SnappedX, preview.SnappedZ);
                }
                else
                {
                    string failureText;
                    if (preview.Failure == RoadToolFailure.Terrain)
                    {
                        failureText = preview.TerrainFailure.ToString();
                    }
                    else
                    {
                        failureText = preview.Failure.ToString();
                    }
                    line2 = $"Invalid: {failureText}";
                }

                _label.Text = $"Road tool: ON (R to toggle)\n{line2}";
            }
            else
            {
                _ghost.Hide();
                _label.Text = "Road tool: ON (R to toggle)\nNo terrain under cursor";
            }
        }
        else
        {
            _ghost.Hide();
            _label.Text = "Road tool: ON (R to toggle)\nNo terrain under cursor";
        }
    }
}