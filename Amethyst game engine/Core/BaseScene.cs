using Amethyst_game_engine.CameraModules;
using Amethyst_game_engine.Core.GameObjects;
using Amethyst_game_engine.Core.GameObjects.Lights;
using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace Amethyst_game_engine.Core;

public abstract class BaseScene : IDisposable
{
    internal event Action<Vector3> ColorUpdate;

    private readonly List<DrawableObject> _objects = [];
    private readonly List<StandartCameraController> _cameraControllers = [];
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Dictionary<string, Camera> _cameras = [];

    private readonly DirectionalLight[] _dirLights;
    private readonly string[] _dirLightsNames;
    private readonly HashSet<string> _unicDirLightsNames = [];
    private readonly Queue<int> _dirLightsFreeSpaces = [];
    private int _countOfDirLights = 0;

    private readonly PointLight[] _pointLights;
    private readonly string[] _pointLightsNames;
    private readonly HashSet<string> _unicPointLightsNames = [];
    private readonly Queue<int> _pointLightsFreeSpaces = [];
    private int _countOfPointLights = 0;

    private readonly Spotlight[] _spotlights;
    private readonly string[] _spotlightsNames;
    private readonly HashSet<string> _unicSpotlightsNames = [];
    private readonly Queue<int> _spotlightsFreeSpaces = [];
    private int _countOfSpotlights = 0;

    private Vector3 _backgroundColor = new(0.3f, 0.3f, 0.3f);
    private bool _disposed;

    private readonly int _dirLightsUBO = GL.GenBuffer();
    private readonly int _pointLightsUBO = GL.GenBuffer();
    private readonly int _spotLightsUBO = GL.GenBuffer();

    public float AspectRatio { get; } = Window.AspectRatio;

    public Vector3 BackgroundColor
    {
        get => _backgroundColor;

        set
        {
            _backgroundColor = value;
            ColorUpdate(value);
        }
    }

    protected BaseScene(int sceneUpdateFPS = 60)
    {
        ColorUpdate += Window.ChangeBackgroundColor;
        ColorUpdate(_backgroundColor);

        OnSceneStart();
        var updateTime = TimeSpan.FromMilliseconds(1000d / sceneUpdateFPS);

        Task.Factory.StartNew(() =>
        {
            while (_cancellationTokenSource.IsCancellationRequested == false)
            {
                OnFixedTimeUpdate();
                Thread.Sleep(updateTime);
            }
        }, _cancellationTokenSource.Token);


        _dirLights = new DirectionalLight[LightManager.MaxDirectionLights];
        _dirLightsNames = new string[LightManager.MaxDirectionLights];

        _pointLights = new PointLight[LightManager.MaxDirectionLights];
        _pointLightsNames = new string[LightManager.MaxDirectionLights];

        _spotlights = new Spotlight[LightManager.MaxDirectionLights];
        _spotlightsNames = new string[LightManager.MaxDirectionLights];

        InitUBO();
    }

    ~BaseScene()
    {
        if (_disposed == false)
            SystemSettings.PrintMessage("Warning. The Dispose method was not called, GPU memory leak", MessageTypes.WarningMessage);
    }

    protected virtual void OnFrameUpdate() { }
    protected virtual void OnFixedTimeUpdate() { }
    protected virtual void OnSceneStart() { }
    protected virtual void OnSceneExit() { }


    internal void DrawScene()
    {
        foreach (var camera in _cameras.Values)
        {
            foreach (var gameObj in _objects)
            {
                gameObj.DrawObject(camera, _countOfDirLights, _countOfPointLights, _countOfSpotlights);
            }
        }

        OnFrameUpdate();
    }

    private void InitUBO()
    {
        GL.BindBuffer(BufferTarget.UniformBuffer, _dirLightsUBO);
        GL.BufferData(BufferTarget.UniformBuffer,
                      Marshal.SizeOf<DirectionalLight>() * LightManager.MaxDirectionLights,
                      nint.Zero, BufferUsageHint.DynamicDraw);

        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, _dirLightsUBO);

        GL.BindBuffer(BufferTarget.UniformBuffer, _pointLightsUBO);
        GL.BufferData(BufferTarget.UniformBuffer,
                      Marshal.SizeOf<PointLight>() * LightManager.MaxPointLights,
                      nint.Zero, BufferUsageHint.DynamicDraw);

        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 1, _pointLightsUBO);

        GL.BindBuffer(BufferTarget.UniformBuffer, _spotLightsUBO);
        GL.BufferData(BufferTarget.UniformBuffer,
                      Marshal.SizeOf<Spotlight>() * LightManager.MaxSpotLights,
                      nint.Zero, BufferUsageHint.DynamicDraw);

        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 2, _spotLightsUBO);
    }

    #region group of methods with cameras
    protected void AddControllerToCamera(StandartCameraController controller, string cameraName)
    {
        if (_cameras.TryGetValue(cameraName, out var camera) == true)
        {
            _cameraControllers.Add(controller);
            controller.BindCamera(camera);
        }
        else
        {
            SystemSettings.PrintMessage("Error. There is no camera with that name", MessageTypes.ErrorMessage);
        }       
    }

    protected void AddCamera(Camera cam, string name)
    {
        if (_cameras.TryAdd(name, cam) == false)
            SystemSettings.PrintMessage("Error. A camera with that name has already been added", MessageTypes.ErrorMessage);
    }

    protected void RemoveCamera(string name)
    {
        if (_cameras.Remove(name) == false)
            SystemSettings.PrintMessage("Error. There is no camera with that name", MessageTypes.ErrorMessage);
    }

    protected Camera GetCamera(string name) => _cameras[name];

    #endregion

    #region group of methods with game objects
    protected void AddGameObject(DrawableObject obj)
    {
        _objects.Add(obj);;
    }

    protected void RemoveGameObjects(Predicate<DrawableObject> predicate)
    {
        foreach (var item in _objects)
        {
            if (predicate(item))
            {
                _objects.Remove(item);
                item.Dispose();
            }
        }
    }

    protected DrawableObject[] GetGameObject(Predicate<DrawableObject> predicate)
    {
        List<DrawableObject> result = [];

        foreach (var item in _objects)
        {
            if (predicate(item))
            {
                result.Add(item);
            }
        }

        return [.. result];
    }

    #endregion

    #region group of methods with lights

    public void RemoveSpotlight(string name)
    {
        for (int i = 0; i < _spotlights.Length; i++)
        {
            var currentSpotlightName = _spotlightsNames[i];

            if (currentSpotlightName == null)
            {
                break;
            }
            else if (currentSpotlightName == name)
            {
                _spotlights[i].color = new(-1);
                _unicSpotlightsNames.Remove(name);
                _spotlightsFreeSpaces.Enqueue(i);

                _countOfSpotlights--;

                return;
            }
        }

        throw new ArgumentException($"Error, there is no spotlight source named \"{name}\"");
    }

    public void AddSpotlight(Vector3 position,
                            float yaw,
                            float pitch,
                            Color color,
                            float intensity,
                            float innerCutoff,
                            float outerCutoff,
                            float constant,
                            float linear,
                            float quadratic,
                            string name)
    {
        if (_unicSpotlightsNames.Contains(name))
            throw new ArgumentException($"Error, the spotlight source named \"{name}\" already exists");

        if (_spotlightsFreeSpaces.Count != 0)
            AddLight(_spotlightsFreeSpaces.Dequeue());
        else if (_countOfSpotlights < LightManager.MaxSpotLights)
            AddLight(_unicSpotlightsNames.Count);
        else
            throw new IndexOutOfRangeException("Error. The limit on the number of spotlight sources has been reached");

        void AddLight(int index)
        {
            var yawRadians = Mathematics.DegreesToRadians(yaw);
            var pitchRadians = Mathematics.DegreesToRadians(pitch);

            var direction = new Vector3(MathF.Cos(yawRadians) * MathF.Cos(pitchRadians),
                                        MathF.Sin(pitchRadians),
                                        MathF.Sin(yawRadians) * MathF.Cos(pitchRadians));

            _spotlights[index] = new()
            {
                position = position,
                direction = direction,
                color = new(color.r, color.g, color.g),
                intensity = intensity,
                innerCutOff = MathF.Cos(Mathematics.DegreesToRadians(innerCutoff)),
                outerCutOff = MathF.Cos(Mathematics.DegreesToRadians(outerCutoff)),
                constant = constant,
                linear = linear,
                quadratic = quadratic
            };

            _spotlightsNames[index] = name;
            _unicSpotlightsNames.Add(name);

            GL.BindBuffer(BufferTarget.UniformBuffer, _spotLightsUBO);
            GL.BufferSubData(BufferTarget.UniformBuffer,
                            Marshal.SizeOf<Spotlight>() * index,
                            Marshal.SizeOf<Spotlight>(),
                            ref _spotlights[index]);

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 2, _spotLightsUBO);

            _countOfSpotlights++;
        }
    }

    public void RemovePointLight(string name)
    {
        for (int i = 0; i < _pointLights.Length; i++)
        {
            var currentPointLightName = _pointLightsNames[i];

            if (currentPointLightName == null)
            {
                break;
            }
            else if (currentPointLightName == name)
            {
                _pointLights[i].color = new(-1);
                _unicPointLightsNames.Remove(name);
                _pointLightsFreeSpaces.Enqueue(i);

                _countOfPointLights--;

                return;
            }
        }

        throw new ArgumentException($"Error, there is no point light source named \"{name}\"");
    }

    public void AddPointLight(Vector3 position, Color color, float intensity, float constant, float linear, float quadratic, string name)
    {
        if (_unicPointLightsNames.Contains(name))
            throw new ArgumentException($"Error, the point light source named \"{name}\" already exists");

        if (_pointLightsFreeSpaces.Count != 0)
            AddLight(_pointLightsFreeSpaces.Dequeue());
        else if (_countOfPointLights < LightManager.MaxPointLights)
            AddLight(_unicPointLightsNames.Count);
        else
            throw new IndexOutOfRangeException("Error. The limit on the number of point light sources has been reached");


        void AddLight(int index)
        {
            _pointLights[index] = new()
            {
                position = position,
                color = new(color.r, color.g, color.b),
                intensity = intensity,
                constant = constant,
                linear = linear,
                quadratic = quadratic
            };

            _pointLightsNames[index] = name;
            _unicPointLightsNames.Add(name);

            GL.BindBuffer(BufferTarget.UniformBuffer, _pointLightsUBO);
            GL.BufferSubData(BufferTarget.UniformBuffer,
                            Marshal.SizeOf<PointLight>() * index,
                            Marshal.SizeOf<PointLight>(),
                            ref _pointLights[index]);

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 1, _pointLightsUBO);

            _countOfPointLights++;
        }
    }

    public void RemoveDirectionalLight(string name)
    {
        for (int i = 0; i < _dirLightsNames.Length; i++)
        {
            var currentDirLightName = _dirLightsNames[i];

            if (currentDirLightName == null)
            {
                break;
            }
            else if (currentDirLightName == name)
            {
                _dirLights[i].color = new(-1);
                _unicDirLightsNames.Remove(name);
                _dirLightsFreeSpaces.Enqueue(i);

                _countOfDirLights--;

                return;
            }
        }

        throw new ArgumentException($"Error, there is no directional light source named \"{name}\"");
    }

    public void AddDirectionalLight(float yaw, float pitch, Color color, float intensity, string name)
    {
        if (_unicDirLightsNames.Contains(name))
            throw new ArgumentException($"Error, the directional light source named \"{name}\" already exists");

        if (_dirLightsFreeSpaces.Count != 0)
            AddLight(_dirLightsFreeSpaces.Dequeue());
        else if (_countOfDirLights < LightManager.MaxDirectionLights)
            AddLight(_unicDirLightsNames.Count);
        else
            throw new IndexOutOfRangeException("Error. The limit on the number of directional light sources has been reached");

        void AddLight(int index)
        {
            var yawRadians = Mathematics.DegreesToRadians(yaw);
            var pitchRadians = Mathematics.DegreesToRadians(pitch);

            var direction = new Vector3(MathF.Cos(yawRadians) * MathF.Cos(pitchRadians),
                                        MathF.Sin(pitchRadians),
                                        MathF.Sin(yawRadians) * MathF.Cos(pitchRadians));

            _dirLights[index] = new()
            {
                direction = direction,
                color = new(color.r, color.g, color.b),
                intensity = intensity
            };

            _dirLightsNames[index] = name;
            _unicDirLightsNames.Add(name);

            GL.BindBuffer(BufferTarget.UniformBuffer, _dirLightsUBO);
            GL.BufferSubData(BufferTarget.UniformBuffer,
                            Marshal.SizeOf<DirectionalLight>() * index,
                            Marshal.SizeOf<DirectionalLight>(),
                            ref _dirLights[index]);

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, _dirLightsUBO);

            _countOfDirLights++;
        }     
    }

    #endregion

    internal void ChangeGlobalRenderSettings(uint globalSettings)
    {
        foreach (var obj in _objects)
        {
            obj.ChangeGlobalRenderSettings(globalSettings);
        }
    }

    public void Dispose()
    {
        if (_disposed == false)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();

            OnSceneExit();

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            foreach (var obj in _objects)
            {
                obj.Dispose();
            }

            foreach (var cam in _cameras.Values)
            {
                cam.Dispose();
            }

            ColorUpdate -= Window.ChangeBackgroundColor;
            Window.ClearInputHandlers();

            GC.SuppressFinalize(this);

            _disposed = true;
        }
    }
}
