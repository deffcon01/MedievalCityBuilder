using Godot;

using MedievalCityBuilder.Core;

namespace MedievalCityBuilder.Presentation;

/// <summary>
/// Camera rig: a yaw pivot with a child Camera3D at a fixed downward pitch.
/// Pan is relative to camera yaw. Q/E rotates yaw. Mouse wheel zooms by
/// changing the camera distance from the pivot. All tunable values are
/// [Export] fields with clearly marked placeholder defaults.
/// </summary>
public partial class CameraRig : Node3D
{
    // --- Tunable values (placeholder defaults) ---

    [Export] private float panSpeed = 8.0f;              // units per second at 1.0 input
    [Export] private float rotateSpeed = 60.0f;          // degrees per second
    [Export] private float zoomMin = 4.0f;               // placeholder: closest distance
    [Export] private float zoomMax = 40.0f;              // placeholder: farthest distance
    [Export] private float zoomStep = 2.0f;              // placeholder: distance change per wheel notch
    [Export] private float pitchDegrees = 55.0f;         // placeholder: downward pitch from horizontal
    [Export] private float startZoom = 16.0f;            // placeholder: initial distance

    [Export] private float boundsMinX = -50.0f;          // placeholder
    [Export] private float boundsMaxX = 50.0f;           // placeholder
    [Export] private float boundsMinZ = -50.0f;          // placeholder
    [Export] private float boundsMaxZ = 50.0f;           // placeholder

    private Camera3D? _camera;
    private float _currentZoom;

    public override void _Ready()
    {
        _camera = GetNode<Camera3D>("Camera3D");
        _currentZoom = startZoom;
        ApplyZoom();
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        // Q/E: rotate yaw
        if (Input.IsKeyPressed(Key.Q))
            RotateY(Mathf.DegToRad(rotateSpeed * dt));
        if (Input.IsKeyPressed(Key.E))
            RotateY(Mathf.DegToRad(-rotateSpeed * dt));

        // WASD / arrows: pan relative to camera yaw
        float pan = 0f;
        float strafe = 0f;

        bool w = Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up);
        bool s = Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down);
        bool a = Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left);
        bool d = Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right);

        if (w) pan += 1f;
        if (s) pan -= 1f;
        if (a) strafe -= 1f;
        if (d) strafe += 1f;

        if (pan != 0f || strafe != 0f)
        {
            float yaw = Rotation.Y;
            // Forward direction in the XZ plane (camera looks along its local -Z).
            Vector3 forward = new Vector3(-Mathf.Sin(yaw), 0f, -Mathf.Cos(yaw));
            Vector3 right = new Vector3(Mathf.Cos(yaw), 0f, -Mathf.Sin(yaw));

            Vector3 move = forward * (pan * panSpeed * dt) + right * (strafe * panSpeed * dt);
            Vector3 pos = Position;
            pos.X += move.X;
            pos.Z += move.Z;

            var clamped = CameraMath.ClampToBounds(pos.X, pos.Z, boundsMinX, boundsMaxX, boundsMinZ, boundsMaxZ);
            pos.X = clamped.x;
            pos.Z = clamped.z;
            Position = pos;
        }
    }

    public override void _Input(InputEvent ev)
    {
        if (ev is InputEventMouseButton mb && mb.Pressed)
        {
            if (mb.ButtonIndex == MouseButton.WheelUp)
            {
                _currentZoom -= zoomStep;
                _currentZoom = CameraMath.ClampZoom(_currentZoom, zoomMin, zoomMax);
                ApplyZoom();
            }
            else if (mb.ButtonIndex == MouseButton.WheelDown)
            {
                _currentZoom += zoomStep;
                _currentZoom = CameraMath.ClampZoom(_currentZoom, zoomMin, zoomMax);
                ApplyZoom();
            }
        }
    }

    private void ApplyZoom()
    {
        if (_camera == null) return;

        float pitchRad = Mathf.DegToRad(pitchDegrees);
        // Camera sits above and behind the pivot (at +Z), looking toward -Z.
        float y = _currentZoom * Mathf.Sin(pitchRad);
        float z = _currentZoom * Mathf.Cos(pitchRad);
        _camera.Position = new Vector3(0f, y, z);
        // LookAt uses GLOBAL coordinates, so once the rig moves away from the
        // origin the camera would spin to track the world origin instead of the
        // pivot. Setting the local rotation directly keeps the fixed pitch.
        _camera.Rotation = new Vector3(-pitchRad, 0f, 0f);
    }
}