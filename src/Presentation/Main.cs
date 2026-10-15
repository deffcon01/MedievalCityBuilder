using Godot;
using MedievalCityBuilder.Persistence;
using MedievalCityBuilder.World;
using System;
using System.IO;

namespace MedievalCityBuilder.Presentation;

public partial class Main : Node3D
{
    private TerrainView _terrainView = null!;
    private CameraRig _cameraRig = null!;

    public override void _Ready()
    {
        GD.Print("MedievalCityBuilder: Main scene loaded");
        GD.Print("Keys: F6 save, F7 load, N new random world");

        _terrainView = GetNode<TerrainView>("TerrainView");
        _cameraRig = GetNode<CameraRig>("CameraRig");

        ApplyCameraBounds();
    }

    private void ApplyCameraBounds()
    {
        var (minX, maxX, minZ, maxZ) = _terrainView.WorldExtentsXZ;
        float margin = 2.0f;
        _cameraRig.SetBounds(minX + margin, maxX - margin, minZ + margin, maxZ - margin);

        _cameraRig.Position = new Vector3(_cameraRig.Position.X, (float)(_terrainView.HeightScaleValue * 0.5), _cameraRig.Position.Z);
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
        {
            return;
        }

        string savePath = ProjectSettings.GlobalizePath("user://saves/world.json");

        switch (keyEvent.Keycode)
        {
            case Key.F6:
                try
                {
                    WorldSaveFile.Save(savePath, _terrainView.CurrentWorld);
                    GD.Print($"Saved world to {savePath}");
                }
                catch (IOException ex)
                {
                    GD.PrintErr($"Failed to save world: {ex.Message}");
                }
                catch (UnauthorizedAccessException ex)
                {
                    GD.PrintErr($"Failed to save world (access denied): {ex.Message}");
                }
                GetViewport().SetInputAsHandled();
                break;

            case Key.F7:
                try
                {
                    var loaded = WorldSaveFile.Load(savePath);
                    _terrainView.ApplyWorld(loaded);
                    ApplyCameraBounds();
                    GD.Print($"Loaded world: seed={loaded.Generation.Seed} size={loaded.Generation.Size}");
                }
                catch (FileNotFoundException)
                {
                    GD.PrintErr($"Save file not found: {savePath}");
                }
                catch (SaveFormatException ex)
                {
                    GD.PrintErr($"Failed to load world (invalid format): {ex.Message}");
                }
                catch (IOException ex)
                {
                    GD.PrintErr($"Failed to load world: {ex.Message}");
                }
                catch (UnauthorizedAccessException ex)
                {
                    GD.PrintErr($"Failed to load world (access denied): {ex.Message}");
                }
                GetViewport().SetInputAsHandled();
                break;

            case Key.N:
                var currentWorld = _terrainView.CurrentWorld;
                var random = new Random(unchecked((int)DateTime.Now.Ticks));
                ulong newSeed = (ulong)random.NextInt64();
                var newGeneration = new WorldGenerationSettings(
                    newSeed,
                    WorldGenerationSettings.CurrentGeneratorVersion,
                    currentWorld.Generation.Size
                );
                var newWorld = new WorldSaveData(
                    newGeneration,
                    TerrainSettings.Default,
                    WaterSettings.Default
                );
                _terrainView.ApplyWorld(newWorld);
                ApplyCameraBounds();
                GD.Print($"New world: seed={newSeed}");
                GetViewport().SetInputAsHandled();
                break;
        }
    }
}