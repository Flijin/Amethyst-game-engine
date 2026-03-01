namespace Amethyst_game_engine.Core.New_classes;

public sealed class GameObjectsManager
{
    private readonly List<DrawableObject> _gameObjects = [];

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

    public void AddGameObject(DrawableObject obj) => _gameObjects.Add(obj);
    public int RemoveGameObject(Predicate<DrawableObject> condition)
    {
        int RemovedCount = 0;

        for (int i = _gameObjects.Count - 1; i >= 0; i--)
        {
            if (condition(_gameObjects[i]))
            {
                _gameObjects.RemoveAt(i);
                RemovedCount++;
            }
        }

        return RemovedCount;
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

    public DrawableObject? GetGameObjectByIndex(int index)
    {
        if (index >= 0 && index < _gameObjects.Count)
            return _gameObjects[index];
        else
            return null;
    }

    public void Clear()
    {
        foreach (var gameObject in _gameObjects)
            gameObject.Dispose();

        _gameObjects.Clear();
    }

    public void Dispose()
    {
        Clear();
    }
}
