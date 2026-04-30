using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using Amethyst_game_engine.Core.GameObjects;
using Amethyst_game_engine.Core.GameObjects.Components;
using Amethyst_game_engine.Core.Managers;
using Amethyst_game_engine.Models.GLBModule;
using Amethyst_game_engine.Models.STLModule;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
namespace Amethyst_game_engine.Core;

public abstract class BaseScene : IDisposable
{
    private SceneManager? _sceneManager;
    private readonly GameObjectManager _gameObjectManager = new();
    private readonly CameraManager _cameraManager = new();
    private readonly LightManager _lightManager = new();

    public GameObjectManager GameObjectManager => _gameObjectManager;
    public CameraManager CameraManager => _cameraManager;
    public LightManager LightManager => _lightManager;
    public SceneManager? SceneManager => _sceneManager;

    public BaseScene() => _gameObjectManager.SetBaseScene(this);

    #region Hooks

    protected internal virtual void OnStart()
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnStart();
    }

    protected internal virtual void Update(float deltaTime)
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.Update(deltaTime);
    }

    protected internal virtual void FixedUpdate(float fixedDeltaTime)
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.FixedUpdate(fixedDeltaTime);
    }
    protected internal virtual void OnExit()
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnExit();
    }

    protected internal virtual void OnPause()
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnPause();
    }

    protected internal virtual void OnResume()
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnResume();
    }

    protected internal virtual void OnKeyDown(KeyboardKeyEventArgs e)
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnKeyDown(e);
    }

    protected internal virtual void OnKeyUp(KeyboardKeyEventArgs e)
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnKeyUp(e);
    }

    protected internal virtual void OnMouseDown(MouseButtonEventArgs e)
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnMouseDown(e);
    }

    protected internal virtual void OnMouseUp(MouseButtonEventArgs e)
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnMouseUp(e);
    }

    protected internal virtual void OnMouseWheel(MouseWheelEventArgs e)
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnMouseWheel(e);
    }

    protected internal virtual void OnMouseMove(MouseMoveEventArgs e)
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnMouseMove(e);
    }

    #endregion

    [MemberNotNull(nameof(_sceneManager))]
    internal void SetSceneManager(SceneManager manager) => _sceneManager = manager;

    internal void DeserializeScene()
    {
        string fullName = Path.Combine(Path.Combine(Environment.CurrentDirectory, "Saves"), $"{GetType().Name}.json");

        if (File.Exists(fullName) == false)
            return;

        string source = File.ReadAllText(fullName);
        using JsonDocument doc = JsonDocument.Parse(source);
        JsonElement root = doc.RootElement;

        List<object> models = [];

        foreach (var model in root.GetProperty("Models").EnumerateArray())
        {
            var path = model.GetString()!;

            if (File.Exists(path) == false)
                SystemCalls.PrintMessage($"Error, file {path} does not exists");

            if (path.Contains(".glb"))
                models.Add(GLBImporter.LoadModel(path)![0]);
            else
                models.Add(STLImporter.LoadModel(path)!);
        }

        foreach (var gameObj in root.GetProperty("GameObjects").EnumerateArray())
        {
            var modelIndex = gameObj.GetProperty("ModelIndex").GetInt32();
            var tag = gameObj.GetProperty("Tag").GetString()!;

            var transformJson = gameObj.GetProperty("Transform");

            float[] position = transformJson.GetProperty("Position").Deserialize<float[]>()!;
            var positionTransrorm = new Vector3(position[0], position[1], position[2]);

            float[] rotation = transformJson.GetProperty("Rotation").Deserialize<float[]>()!;
            var rotationTransform = new Vector3(rotation[0], rotation[1], rotation[2]);

            float[] scale = transformJson.GetProperty("Scale").Deserialize<float[]>()!;
            var scaleTransrorm = new Vector3(scale[0], scale[1], scale[2]);

            if (gameObj.GetProperty("Type").GetString() == "GLB")
            {
                var glbModelIndex = gameObj.GetProperty("GLBModelIndex").GetInt32();

                GLBGameObject currentObject = new(((GLBScene)models[modelIndex]).Models[glbModelIndex])
                {
                    Tag = tag,
                };

                GameObjectManager.AddGameObject(currentObject);
            }
            else
            {
                STLGameObject currentObject = new((STLModel)models[modelIndex])
                {
                    Tag = tag,
                };

                GameObjectManager.AddGameObject(currentObject);
            }

            var lastObject = GameObjectManager.GameObjects[^1];

            lastObject.Transform.Position = positionTransrorm;
            lastObject.Transform.Rotation = rotationTransform;
            lastObject.Transform.Scale = scaleTransrorm;
        }
    }

    internal void SerializeScene()
    {
        string savesFolder = Path.Combine(Environment.CurrentDirectory, "Saves");

        if (Directory.Exists(savesFolder) == false)
            Directory.CreateDirectory(savesFolder);

        string fullName = Path.Combine(savesFolder, $"{GetType().Name}.json");

        JsonObject sceneJson = [];
        JsonArray gameObjectsJson = [];
        JsonArray modelsJson = [];

        foreach (var model in GameObjectManager.Models)
        {
            modelsJson.Add(JsonValue.Create(model));
        }

        foreach (var gameObj in GameObjectManager.GameObjects)
        {
            JsonObject gameObjectJson = new()
            {
                ["Tag"] = JsonValue.Create(gameObj.Tag ?? "Untagged"),
                ["Type"] = JsonValue.Create(gameObj is STLGameObject ? "STL" : "GLB"),
                ["ModelIndex"] = JsonValue.Create(gameObj.ModelIndex),
                ["GLBModelIndex"] = JsonValue.Create(gameObj is GLBGameObject glb ? glb.GLBModelIndex : -1),
                ["Transform"] = new JsonObject()
                {
                    ["Position"] = new JsonArray()
                    {
                        JsonValue.Create(gameObj.Transform.Position.X),
                        JsonValue.Create(gameObj.Transform.Position.Y),
                        JsonValue.Create(gameObj.Transform.Position.Z)
                    },
                    ["Rotation"] = new JsonArray()
                    {
                        JsonValue.Create(gameObj.Transform.Rotation.X),
                        JsonValue.Create(gameObj.Transform.Rotation.Y),
                        JsonValue.Create(gameObj.Transform.Rotation.Z)
                    },
                    ["Scale"] = new JsonArray()
                    {
                        JsonValue.Create(gameObj.Transform.Scale.X),
                        JsonValue.Create(gameObj.Transform.Scale.Y),
                        JsonValue.Create(gameObj.Transform.Scale.Z)
                    }
                }
            };

            gameObjectsJson.Add(gameObjectJson);
        }

        sceneJson.Add("Models", modelsJson);
        sceneJson.Add("GameObjects", gameObjectsJson);

        File.WriteAllText(fullName, sceneJson.ToJsonString(new JsonSerializerOptions()
        {
            WriteIndented = true
        }));
    }

#pragma warning disable CA1816
    internal void Cleanup()
    {
        _gameObjectManager.Cleanup();
        _cameraManager.Cleanup();
        _lightManager.Cleanup();

        GC.SuppressFinalize(this);
    }

    void IDisposable.Dispose()
    {
        Cleanup();
    }
#pragma warning restore
}
