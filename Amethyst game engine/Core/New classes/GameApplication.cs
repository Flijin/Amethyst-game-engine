#define DEBUG_MODE

using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Amethyst_game_engine.Core.New_classes;

public sealed class GameApplication(NativeWindowSettings settings, float tickTime = 1f / 60f) : GameWindow(GameWindowSettings.Default, settings)
{
    private struct FPSCounter
    {
        public int FrameCount { get; private set; }
        public float FpsTime { get; private set; }

        public void Reset()
        {
            FrameCount = 0;
            FpsTime = 0;
        }

        public void UpdateValues(float fpsTime)
        {
            FrameCount++;
            FpsTime += fpsTime;
        }

        public readonly float GetFPS() => FpsTime / FrameCount;
    }

    private readonly SceneManager _sceneManager = new();
    private readonly RenderSystem _renderSystem = new();

    private float _accumulator;
    private readonly float _tickTime = Mathematics.Clamp(tickTime, 1f / 300f, 1f);

    public bool IsSceneLoaded => _sceneManager.CurrentScene != null;
    public ISceneManager SceneManager => _sceneManager;

#if DEBUG_MODE
    private FPSCounter _counter;

#endif

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
    }

    protected override void OnUnload()
    {
        _sceneManager.UnloadScene();
        _sceneManager.Dispose();

        base.OnUnload();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, Size.X, Size.Y);
        _sceneManager?.CurrentScene?.CameraManager?.UpdateAspectRatio((float)Size.X / Size.Y);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        float deltaTime = (float)args.Time;

#if DEBUG_MODE
        _counter.UpdateValues((float)args.Time);

        if (_counter.FpsTime >= 1)
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

        _sceneManager.Update((float)args.Time);
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
}
