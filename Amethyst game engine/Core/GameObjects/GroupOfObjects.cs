using Amethyst_game_engine.CameraModules;
using Amethyst_game_engine.Models.GLBModule;
using Amethyst_game_engine.Render;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core.GameObjects;

public class GroupOfObjects : DrawableObject, IDisposable
{
    private bool _disposed = false;
    private readonly DrawableObject[] _gameObjects;

    public DrawableObject[] GroupObjects => _gameObjects;

    public override sealed Vector3 Position
    {
        get => base.Position;

        set
        {
            _position += value;

            foreach (var gameObject in _gameObjects)
            {
                if (gameObject is GameObject)
                    gameObject.Position += value;
                else
                    gameObject.Position = value;
            }
        }
    }

    public override sealed Vector3 Rotation
    {
        get => base.Rotation;

        set
        {
            _rotation += value;

            foreach (var gameObject in _gameObjects)
            {
                if (gameObject is GameObject)
                    gameObject.Rotation += value;
                else
                    gameObject.Rotation = value;
            }
        }
    }

    public override sealed Vector3 Scale
    {
        get => base.Scale;

        set
        {
            _scale += value;

            foreach (var gameObject in _gameObjects)
            {
                if (gameObject is GameObject)
                    gameObject.Scale += value;
                else
                    gameObject.Scale = value;
            }
        }
    }

    public GroupOfObjects(DrawableObject[] objects) => _gameObjects = objects;

    public GroupOfObjects(GLBScene scene)
    {
        _gameObjects = new DrawableObject[scene.ModelsCount];

        for (int i = 0; i < scene.ModelsCount; i++)
        {
            _gameObjects[i] = new GLBObject(scene.Models[i], true);
        }
    }

    ~GroupOfObjects()
    {
        if (_disposed == false)
            SystemSettings.PrintMessage("Warning. The Dispose method was not called, RAM memory leak", MessageTypes.WarningMessage);
    }

    public override sealed void ModifyObject(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        base.ModifyObject(position + base.Position, rotation + base.Rotation, scale + base.Scale);

        foreach (var gameObject in _gameObjects)
        {
            gameObject.ModifyObject(position + gameObject.Position, rotation + gameObject.Rotation, scale + gameObject.Scale);
        }
    }

    internal override sealed void DrawObject(Camera? cam, int countOfDirLights, int countOfPointLights, int countOfSpotLights)
    {
        foreach (var gameObject in _gameObjects)
        {
            gameObject.DrawObject(cam, countOfDirLights, countOfPointLights, countOfSpotLights);
        }
    }
    public override sealed void ChangeRenderSettings(RenderSettings settings)
    {
        foreach (var gameObject in _gameObjects)
        {
            gameObject.ChangeRenderSettings(settings);
        }
    }

    internal sealed override void UpdateShaders()
    {
        foreach (var gameObject in _gameObjects)
        {
            gameObject.UpdateShaders();
        }
    }

    public override sealed void Dispose()
    {
        if (_disposed == false)
        {
            base.Dispose();

            foreach (var gameObject in _gameObjects)
            {
                gameObject.Dispose();
            }

            GC.SuppressFinalize(this);
            _disposed = true;
        }
    }
}
