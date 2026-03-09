#define DEBUG_MODE

using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Amethyst_game_engine.Core.New_classes;

public sealed class GameApplication : GameWindow, IGameApplication
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

    public float WindowAspectRatio => _windowAspectRatio;

    public bool IsSceneLoaded => _sceneManager.CurrentScene is not null;
    public ISceneManager SceneManager => _sceneManager;

    public bool ClearBackground
    {
        get => _renderSystem.ClearBackground;
        set => _renderSystem.ClearBackground = value;
    }

#if DEBUG_MODE
    private FPSCounter _counter;

#endif

    public GameApplication(NativeWindowSettings settings, float tickTime = 1f / 60f) :
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
            _sceneManager.FixedUpdate(_tickTime);
            _accumulator -= _tickTime;
        }

        _sceneManager.Update(deltaTime);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        
        var currentScene = _sceneManager.CurrentScene;

        if (currentScene != null)
        {
            _renderSystem.Render(currentScene);
        }

        SwapBuffers();
    }

    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);
        _sceneManager.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyboardKeyEventArgs e)
    {
        base.OnKeyUp(e);
        _sceneManager.OnKeyUp(e);
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        _sceneManager.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
        _sceneManager.OnMouseUp(e);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        _sceneManager.OnMouseWheel(e);
    }
}
