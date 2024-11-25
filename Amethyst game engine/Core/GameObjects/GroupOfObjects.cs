using Amethyst_game_engine.CameraModules;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core.GameObjects;

public class GroupOfObjects(DrawableObject[] objects) : DrawableObject, IDisposable
{
    private bool _disposed = false;
    private readonly DrawableObject[] _gameObjects = objects;

    public override sealed Vector3 Position
    {
        get => base.Position;

        set
        {
            _position += value;

            foreach (var item in _gameObjects)
            {
                if (item is GameObject)
                    item.Position += value;
                else
                    item.Position = value;
            }
        }
    }

    public override sealed Vector3 Rotation
    {
        get => base.Rotation;

        set
        {
            _rotation += value;

            foreach (var item in _gameObjects)
            {
                if (item is GameObject)
                    item.Rotation += value;
                else
                    item.Rotation = value;
            }
        }
    }

    public override sealed Vector3 Scale
    {
        get => base.Scale;

        set
        {
            _scale += value;

            foreach (var item in _gameObjects)
            {
                if (item is GameObject)
                    item.Scale += value;
                else
                    item.Scale = value;
            }
        }
    }

    ~GroupOfObjects()
    {
        if (_disposed == false)
            SystemSettings.PrintErrorMessage("Warning. The Dispose method was not called, RAM memory leak");
    }

    public override sealed void ModifyObject(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        base.ModifyObject(position + base.Position, rotation + base.Rotation, scale + base.Scale);

        foreach (var item in _gameObjects)
        {
            item.ModifyObject(position + item.Position, rotation + item.Rotation, scale + item.Scale);
        }
    }

    internal override sealed void DrawObject(Camera? cam)
    {
        foreach (var gameObject in _gameObjects)
        {
            gameObject.DrawObject(cam);
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
