using System.Diagnostics.CodeAnalysis;

namespace Amethyst_game_engine.Core.New_classes;

public sealed class GameObjectManager
{
    private readonly List<DrawableObject> _gameObjects = [];
    private BaseScene? _scene;

    public IReadOnlyList<DrawableObject> GameObjects => _gameObjects;
    public int Count => _gameObjects.Count;

    internal void OnStart()
    {
        foreach (var gameObj in _gameObjects)
            gameObj.OnStart();
    }

    internal void Update(float deltaTime)
    {
        foreach (var gameObj in _gameObjects)
            gameObj.Update(deltaTime);
    }

    internal void FixedUpdate(float fixedDeltaTime)
    {
        foreach (var gameObj in _gameObjects)
            gameObj.FixedUpdate(fixedDeltaTime);
    }

    internal void OnExit()
    {
        foreach (var gameObj in _gameObjects)
            gameObj.OnExit();
    }

    internal void OnPause()
    {
        foreach (var gameObj in _gameObjects)
            gameObj.OnPause();
    }

    internal void OnResume()
    {
        foreach (var gameObj in _gameObjects)
            gameObj.OnResume();
    }

    public void Clear() => _gameObjects.Clear();
    public int RemoveGameObjects(Predicate<DrawableObject> condition) => _gameObjects.RemoveAll(condition);

    [MemberNotNull(nameof(_scene))]
    internal void SetBaseScene(BaseScene scene) => _scene = scene;

    public void AddGameObject(DrawableObject obj)
    {
        _gameObjects.Add(obj);
        obj.SetScene(_scene!);
    }

    public bool RemoveGameObjectByIndex(int index)
    {
        if (index >= 0 && index < _gameObjects.Count)
        {
            _gameObjects.RemoveAt(index);

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

    public void Dispose()
    {
        foreach (var gameObject in _gameObjects)
            gameObject.Dispose();

    }
}
