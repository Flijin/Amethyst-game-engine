#define DEBUG_MODE

using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Amethyst_game_engine.Core.New_classes;

public class GameApplication(NativeWindowSettings settings, float tickTime = 1f / 60f) : GameWindow(GameWindowSettings.Default, settings)
{
    private readonly RenderSystem _renderSystem = new();
    private readonly SceneManager _sceneManager = new();

    private float _accumulator;
    private readonly float _tickTime = tickTime;

    public bool IsSceneLoaded => _sceneManager.CurrentScene != null;

#if DEBUG_MODE
    private int _frameCounter;
    private float _fpsTimer;
#endif

    public void LoadScene(BaseScene scene) => _sceneManager.LoadScene(scene);
    public void UnloadScene() => _sceneManager.UnloadScene();

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
        _fpsTimer += deltaTime;
        _frameCounter++;

        if (_fpsTimer >= 1)
        {
            Console.WriteLine($"FPS: {_frameCounter / _fpsTimer}");

            _fpsTimer = 0;
            _frameCounter = 0;
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
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        var currentScene = _sceneManager.CurrentScene;

        if (currentScene != null)
        {
            _renderSystem.Render(currentScene);
        }

        SwapBuffers();
    }
}
