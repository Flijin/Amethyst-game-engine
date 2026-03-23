#define DEBUG_MODE

using Amethyst_game_engine.Core.Managers;
using Amethyst_game_engine.Core.Render;
using Amethyst_game_engine.Core.Render.Settings;
using Amethyst_game_engine.Core.Utilities;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Amethyst_game_engine.Core;

public class GameApplication : GameWindow
{
    private struct FPSCounter
    {
        public int FrameCount { get; private set; }
        public float FPSTime { get; private set; }

        public void Reset()
        {
            FrameCount = 0;
            FPSTime = 0;
        }

        public void UpdateValues(float fpsTime)
        {
            FrameCount++;
            FPSTime += fpsTime;
        }

        public readonly float GetFPS() => FrameCount / FPSTime;
    }

    public event Action<float>? ChangedAspectRatio;

    private readonly SceneManager _sceneManager = new();
    private readonly RenderSystem _renderSystem = new();

    private float _accumulator;
    private readonly float _tickTime;
    private float _windowAspectRatio;

    private RenderSettings _renderSettings = RenderSettings.All;
    private ShadingModels _shadingModel = ShadingModels.BlinnPhong;
    private GlobalRenderSettings _globalSettings = new()
    {
        AmbientStrength = 0.1f,
        MaxShininess = 32,
        UseMonochromeAmbient = true,
    };

    public float WindowAspectRatio => _windowAspectRatio;

    public bool IsSceneLoaded => _sceneManager.CurrentScene is not null;
    public SceneManager SceneManager => _sceneManager;

    public bool ClearBackground
    {
        get => _renderSystem.ClearBackground;
        set => _renderSystem.ClearBackground = value;
    }

    public RenderSettings Settings
    {
        get => _renderSettings;

        set
        {
            _renderSettings = value;
            UpdateApplicationSettings();
        }
    }

    public ShadingModels ShadingModel
    {
        get => _shadingModel;

        set
        {
            _shadingModel = value;
            UpdateApplicationSettings();
        }
    }

    public GlobalRenderSettings GlobalSettings
    {
        get => _globalSettings;

        set
        {
            _globalSettings = value;
            UpdateApplicationSettings();
        }
    }

#if DEBUG_MODE
    private FPSCounter _counter;

#endif

    public GameApplication(NativeWindowSettings settings, float tickTime = 1.0f / 60.0f) :
        base(GameWindowSettings.Default, settings)
    {
        _tickTime = Mathematics.Clamp(tickTime, 1.0f / 300.0f, 1.0f);
        _sceneManager.SetGameApplication(this);
        _windowAspectRatio = (float)ClientSize.X / ClientSize.Y;
    }

    public void LoadScene(BaseScene scene) => _sceneManager.LoadScene(scene);
    public void UnloadScene() => _sceneManager.UnloadScene();
    public void SetBackgroundColor(Color color) => _renderSystem.BackgroundColor = color;

    public void SetVSync(bool enabled) => VSync = enabled ? VSyncMode.On : VSyncMode.Off;

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _sceneManager.SetGameApplication(this);
    }

    protected override void OnUnload()
    {
        _sceneManager.UnloadScene();
        _sceneManager.CleanUp();
        ChangedAspectRatio = null;

        ShadersPool.Dispose();

        base.OnUnload();
    } 

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, Size.X, Size.Y);
        _windowAspectRatio = (float)Size.X / Size.Y;

        ChangedAspectRatio?.Invoke(_windowAspectRatio);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        float deltaTime = (float)args.Time;

#if DEBUG_MODE
        _counter.UpdateValues(deltaTime);

        if (_counter.FPSTime >= 1)
        {
            Console.WriteLine($"FPS: {_counter.GetFPS()}");

            _counter.Reset();
        }
#endif
        _accumulator += deltaTime;

        while (_accumulator >= _tickTime)
        {
            _sceneManager.CurrentScene?.FixedUpdate(_tickTime);
            _accumulator -= _tickTime;
        }

        _sceneManager.CurrentScene?.Update(deltaTime);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        
        var currentScene = _sceneManager.CurrentScene;

        if (currentScene is not null)
        {
            _renderSystem.Render(currentScene);
        }

        SwapBuffers();
    }

    #region InputHooks
    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);
        _sceneManager.CurrentScene?.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyboardKeyEventArgs e)
    {
        base.OnKeyUp(e);
        _sceneManager.CurrentScene?.OnKeyUp(e);
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        _sceneManager.CurrentScene?.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
        _sceneManager.CurrentScene?.OnMouseUp(e);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        _sceneManager.CurrentScene?.OnMouseWheel(e);
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        base.OnMouseMove(e);
        _sceneManager.CurrentScene?.OnMouseMove(e);
    }

    #endregion

    private void UpdateApplicationSettings()
    {
        var gameObjects = SceneManager?.CurrentScene?.GameObjectManager.GameObjects;

        if (gameObjects is not null)
        {
            foreach (var gameObj in gameObjects)
            {
                gameObj.UpdateRenderSettings();
            }
        }
    }
}
