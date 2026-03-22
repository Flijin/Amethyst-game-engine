using System.Diagnostics.CodeAnalysis;

namespace Amethyst_game_engine.Core.New_classes;

public sealed class GameObjectManager: IDisposable
{
    public event Action<DrawableObject>? GameObjectAdded;
    public event Action<DrawableObject>? GameObjectRemoved;
    public event Action? OnClear;

    private readonly List<DrawableObject> _gameObjects = [];
    private BaseScene? _scene;

    public IReadOnlyList<DrawableObject> GameObjects => _gameObjects;
    public int Count => _gameObjects.Count;

    [MemberNotNull(nameof(_scene))]
    internal void SetBaseScene(BaseScene scene) => _scene = scene;

    public int RemoveGameObjects(Predicate<DrawableObject> condition)
    {
        for (int i = _gameObjects.Count - 1; i > 0; i++)
        {
            if (condition(_gameObjects[i]))
            {
                GameObjectRemoved?.Invoke(_gameObjects[i]);
                _gameObjects[i].Cleanup();
            }
        }

        return _gameObjects.RemoveAll(condition);
    }

    public void AddGameObject(DrawableObject obj)
    {
        if (_gameObjects.Contains(obj))
        {
            System.PrintMessage("Error. Game object is already exists", MessageTypes.ErrorMessage);
            return;
        }

        _gameObjects.Add(obj);
        obj.SetScene(_scene!);
        obj.UpdateRenderSettings();

        GameObjectAdded?.Invoke(obj);
    }

    public bool RemoveGameObjectAt(int i)
    {
        if (i >= 0 && i < _gameObjects.Count)
        {
            GameObjectRemoved?.Invoke(_gameObjects[i]);
            
            _gameObjects[i].Cleanup();
            _gameObjects.RemoveAt(i);

            return true;
        }

        return false;
    }

    public IEnumerable<DrawableObject> FindGameObjects(Predicate<DrawableObject> condition)
    {
        foreach (var gameObj in _gameObjects)
        {
            if (condition(gameObj))
                yield return gameObj;
        }
    }

    public DrawableObject? GetGameObjectAt(int index)
    {
        if (index >= 0 && index < _gameObjects.Count)
            return _gameObjects[index];
        else
            return null;
    }

    public void Clear()
    {
        Cleanup();
        _gameObjects.Clear();

        OnClear?.Invoke();
    }

    internal void Cleanup()
    {
        foreach (var gameObject in _gameObjects)
            gameObject.Cleanup();
    }

    void IDisposable.Dispose()
    {
        Cleanup();
    }
}
