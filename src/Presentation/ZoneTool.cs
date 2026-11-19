using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;

using MedievalCityBuilder.World;
using MedievalCityBuilder.Zoning;

namespace MedievalCityBuilder.Presentation;

/// <summary>
/// Zone tool presentation node. Handles input, terrain picking, brush ghost, and zone overlay display.
/// All zone painting rules live in src/Zoning; this node only handles presentation.
/// </summary>
public partial class ZoneTool : Node3D
{
    private TerrainView? _terrain;
    private ZoneGrid? _grid;
    private ZoneRegistry? _registry;
    private MeshInstance3D? _ghost;
    private StandardMaterial3D? _ghostMaterial;
    private CanvasLayer? _labelLayer;
    private Label? _label;
    private Node3D? _visuals;
    private MeshInstance3D? _overlayInstance;
    private StandardMaterial3D? _overlayMaterial;

    // PLACEHOLDER values per requirements
    private const double DefaultCellSize = 1.0;
    private const double HeightOffset = 0.15;
    private const double MinBrushRadius = 0.5;
    private const double MaxBrushRadius = 8.0;
    private const double BrushStep = 0.5;

    private double _brushRadius = 2.0; // PLACEHOLDER initial brush radius
    private ZoneTypeId _selectedZoneId = new ZoneTypeId(1); // Small Residential
    private string _lastMessage = "";
    private bool _active = false;
    private bool _gridChanged = true;

    public bool Active => _active;

    /// <summary>
    /// Attempts to activate the zone tool. Returns false (and logs a message) if the
    /// road tool is already active.
    /// </summary>
    public bool TryActivate()
    {
        if (RoadTool.ActiveTool == ToolMode.Road)
        {
            _lastMessage = "Turn off the road tool first (R)";
            GD.Print(_lastMessage);
            return false;
        }

        if (_active)
        {
            return true;
        }

        RoadTool.ActiveTool = ToolMode.Zone;
        _active = true;

        if (_ghost != null) _ghost.Visible = _active;
        if (_labelLayer != null) _labelLayer.Visible = _active;
        if (_overlayInstance != null) _overlayInstance.Visible = _active;

        return true;
    }

    /// <summary>
    /// Initializes the zone tool with the given terrain view.
    /// Logs exactly one line: ZoneTool: ready (Z toggles zone tool)
    /// </summary>
    public void Initialize(TerrainView terrain)
    {
        _terrain = terrain ?? throw new ArgumentNullException(nameof(terrain));
        _terrain.WorldApplied += OnWorldApplied;

        _registry = ZoneRegistry.CreateStartingRegistry();

        BuildGhost();
        BuildLabel();
        BuildVisuals();

        ResetForWorld();

        GD.Print("ZoneTool: ready (Z toggles zone tool)");
    }

    private void OnWorldApplied()
    {
        ResetForWorld();
    }

    private void BuildVisuals()
    {
        _visuals = new Node3D
        {
            Name = "ZoneVisuals",
            TopLevel = true
        };
        AddChild(_visuals);

        _overlayInstance = new MeshInstance3D
        {
            Name = "ZoneOverlay",
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };
        _overlayInstance.Hide();
        _visuals.AddChild(_overlayInstance);

        // Unshaded material with transparency and vertex colors
        _overlayMaterial = new StandardMaterial3D
        {
            ShadingMode = StandardMaterial3D.ShadingModeEnum.Unshaded,
            Transparency = StandardMaterial3D.TransparencyEnum.Alpha,
            VertexColorUseAsAlbedo = true,
            CullMode = StandardMaterial3D.CullModeEnum.Disabled
        };
    }

    private void BuildGhost()
    {
        // Flat disc: CylinderMesh with small height and radius
        var cylinderMesh = new CylinderMesh
        {
            TopRadius = 1.0f,
            BottomRadius = 1.0f,
            Height = 0.05f,
            RadialSegments = 32
        };

        _ghostMaterial = new StandardMaterial3D
        {
            ShadingMode = StandardMaterial3D.ShadingModeEnum.Unshaded,
            Transparency = StandardMaterial3D.TransparencyEnum.Alpha,
            AlbedoColor = new Color(0.2f, 0.8f, 0.3f, 0.55f) // Greenish transparent
        };

        _ghost = new MeshInstance3D
        {
            Mesh = cylinderMesh,
            Name = "ZoneGhost",
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
            Position = new Vector2(12, 120),
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
        if (_terrain == null || _registry == null) return;

        // Create grid covering terrain extents rounded up
        var extents = _terrain.WorldExtentsXZ;
        double width = extents.maxX - extents.minX;
        double depth = extents.maxZ - extents.minZ;

        int cellsX = (int)Math.Ceiling(width / DefaultCellSize);
        int cellsZ = (int)Math.Ceiling(depth / DefaultCellSize);

        _grid = new ZoneGrid(_registry, extents.minX, extents.minZ, DefaultCellSize, cellsX, cellsZ);
        _gridChanged = true;

        if (_ghost != null) _ghost.Hide();
        if (_label != null)
        {
            _label.Hide();
            _label.Text = "";
        }
        if (_overlayInstance != null) _overlayInstance.Hide();
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            if (keyEvent.Keycode == Key.Z)
            {
                if (_active)
                {
                    _active = false;
                    RoadTool.ActiveTool = ToolMode.None;

                    if (_ghost != null) _ghost.Hide();
                    if (_labelLayer != null) _labelLayer.Visible = false;
                    if (_overlayInstance != null) _overlayInstance.Hide();
                    _lastMessage = "";
                    if (_label != null) _label.Text = "";
                }
                else
                {
                    if (TryActivate())
                    {
                        if (_labelLayer != null) _labelLayer.Visible = true;
                        if (_overlayInstance != null) _overlayInstance.Visible = true;
                    }
                }

                GetViewport().SetInputAsHandled();
                return;
            }

            if (_active && RoadTool.ActiveTool == ToolMode.Zone)
            {
                // Number keys 1-8 select a zone from the registry by registry order
                Key key = keyEvent.Keycode;
                if (key >= Key.Key1 && key <= Key.Key8)
                {
                    int index = (int)(key - Key.Key1);
                    var defs = _registry?.Definitions;
                    if (defs != null && index < defs.Count)
                    {
                        _selectedZoneId = defs[index].Id;
                        _lastMessage = $"Selected zone: {defs[index].Name}";
                        GetViewport().SetInputAsHandled();
                        return;
                    }
                }

                // [ and ] change brush radius by 0.5 within [0.5, 8.0]
                if (key == Key.Less)
                {
                    _brushRadius = Math.Max(MinBrushRadius, _brushRadius - BrushStep);
                    _lastMessage = string.Format(CultureInfo.InvariantCulture, "Brush radius: {0:F1}", _brushRadius);
                    GetViewport().SetInputAsHandled();
                    return;
                }
                if (key == Key.Greater)
                {
                    _brushRadius = Math.Min(MaxBrushRadius, _brushRadius + BrushStep);
                    _lastMessage = string.Format(CultureInfo.InvariantCulture, "Brush radius: {0:F1}", _brushRadius);
                    GetViewport().SetInputAsHandled();
                    return;
                }
            }
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

        return TerrainRaycaster.TryRaycast(
            _terrain.Surface,
            (double)origin.X, (double)origin.Y, (double)origin.Z,
            (double)dir.X, (double)dir.Y, (double)dir.Z,
            300.0, 0.5,
            out hit);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!_active || RoadTool.ActiveTool != ToolMode.Zone)
        {
            return;
        }

        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left || mouseEvent.ButtonIndex == MouseButton.Right)
            {
                ApplyBrushAtMouse(mouseEvent.ButtonIndex == MouseButton.Left);
                GetViewport().SetInputAsHandled();
                return;
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!_active || RoadTool.ActiveTool != ToolMode.Zone)
        {
            return;
        }

        // Continuous painting/erasing while button is held and mouse moves
        if (@event is InputEventMouseMotion)
        {
            if (Input.IsMouseButtonPressed(MouseButton.Left))
            {
                ApplyBrushAtMouse(true);
            }
            else if (Input.IsMouseButtonPressed(MouseButton.Right))
            {
                ApplyBrushAtMouse(false);
            }
        }
    }

    private void ApplyBrushAtMouse(bool isPaint)
    {
        if (_grid == null || _terrain == null) return;

        if (!TryPick(out var hit) || !_terrain.Surface.Contains(hit.X, hit.Z))
        {
            return;
        }

        int changed = isPaint
            ? _grid.Paint(hit.X, hit.Z, _brushRadius, _selectedZoneId)
            : _grid.Erase(hit.X, hit.Z, _brushRadius);

        if (changed > 0)
        {
            _gridChanged = true;
            _lastMessage = isPaint ? $"Painted {changed} cells" : $"Erased {changed} cells";
        }
    }

    public override void _Process(double delta)
    {
        if (!_active || RoadTool.ActiveTool != ToolMode.Zone || _terrain == null || _grid == null || _registry == null || _ghost == null || _ghostMaterial == null || _label == null)
        {
            return;
        }

        bool hasHit = TryPick(out var hit);

        // Update ghost position and visibility
        if (hasHit && _terrain.Surface.Contains(hit.X, hit.Z))
        {
            double y = _terrain.Surface.SampleHeight(hit.X, hit.Z) + 0.05;
            _ghost.GlobalPosition = new Vector3((float)hit.X, (float)y, (float)hit.Z);
            // Scale cylinder mesh radius: diameter = 2 * radius
            _ghost.Scale = new Vector3((float)(_brushRadius * 2.0), 1.0f, (float)(_brushRadius * 2.0));
            _ghost.Show();
        }
        else
        {
            _ghost.Hide();
        }

        // Rebuild overlay if grid changed
        if (_gridChanged && _overlayInstance != null)
        {
            RebuildOverlay();
            _gridChanged = false;
        }

        // Build label text
        var lines = new List<string>();
        lines.Add("Zone tool: ON (Z toggle, 1-8 zone, [/] radius, LMB paint, RMB erase)");

        string zoneName = "None";
        if (_registry.TryGet(_selectedZoneId, out var def))
        {
            zoneName = def.Name;
        }
        lines.Add(string.Format(CultureInfo.InvariantCulture, "Selected zone: {0} ({1})", zoneName, _selectedZoneId));
        lines.Add(string.Format(CultureInfo.InvariantCulture, "Brush radius: {0:F1} units", _brushRadius));
        lines.Add(string.Format(CultureInfo.InvariantCulture, "Painted cells: {0}", _grid.PaintedCellCount));

        if (!string.IsNullOrEmpty(_lastMessage))
        {
            lines.Add(_lastMessage);
        }

        _label.Text = string.Join("\n", lines);
    }

    private void RebuildOverlay()
    {
        if (_grid == null || _terrain == null || _overlayInstance == null || _registry == null) return;

        var meshData = ZoneOverlayBuilder.Build(_grid, _terrain.Surface, HeightOffset);

        if (meshData.VertexCount == 0)
        {
            _overlayInstance.Mesh = null;
            return;
        }

        int vertexCount = meshData.VertexCount;
        var vertices = new Vector3[vertexCount];
        var normals = new Vector3[vertexCount];
        var colors = new Color[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            int pi = i * 3;
            vertices[i] = new Vector3(meshData.Positions[pi], meshData.Positions[pi + 1], meshData.Positions[pi + 2]);
            normals[i] = new Vector3(0.0f, 1.0f, 0.0f);

            // Color by zone category
            int zoneIdValue = meshData.VertexZoneIds[i];
            colors[i] = GetZoneColor(new ZoneTypeId(zoneIdValue));
        }

        var arrayMesh = new ArrayMesh();
        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = vertices;
        arrays[(int)Mesh.ArrayType.Normal] = normals;
        arrays[(int)Mesh.ArrayType.Color] = colors;
        arrays[(int)Mesh.ArrayType.Index] = meshData.Indices;

        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        _overlayInstance.Mesh = arrayMesh;
        _overlayInstance.SetSurfaceOverrideMaterial(0, _overlayMaterial);
    }

    private Color GetZoneColor(ZoneTypeId zoneId)
    {
        if (_registry == null || !_registry.TryGet(zoneId, out var def))
        {
            return new Color(0.5f, 0.5f, 0.5f, 0.55f);
        }

        // Placeholder colors: residential green, commercial blue, industrial orange, mixed-use purple
        // Alpha about 0.55
        return def.Category switch
        {
            ZoneCategory.Residential => new Color(0.2f, 0.8f, 0.3f, 0.55f),    // Green
            ZoneCategory.Commercial => new Color(0.2f, 0.5f, 0.9f, 0.55f),     // Blue
            ZoneCategory.Industrial => new Color(0.9f, 0.5f, 0.1f, 0.55f),     // Orange
            ZoneCategory.MixedUse => new Color(0.7f, 0.3f, 0.8f, 0.55f),       // Purple
            _ => new Color(0.5f, 0.5f, 0.5f, 0.55f)
        };
    }
}