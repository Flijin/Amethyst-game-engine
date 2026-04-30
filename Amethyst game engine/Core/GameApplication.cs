#define DEBUG_MODE

using Amethyst_game_engine.Core.Managers;
using Amethyst_game_engine.Core.Render;
using Amethyst_game_engine.Core.Render.Settings;
using Amethyst_game_engine.Core.Utilities;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Amethyst_game_engine.Core;

public class GameApplication : GameWindow
{
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

    private readonly bool _editMode;
    private float _fpsTime;
    private int _frameCount;

    private EditorWindow? _editorWindow;

    public float WindowAspectRatio => _windowAspectRatio;

    public bool IsSceneLoaded => _sceneManager.CurrentScene is not null;
    public SceneManager SceneManager => _sceneManager;

    public int FPS { get; private set; }

    public bool EditMode
    {
        get => _editMode;
    }

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

    public GameApplication(string title, bool editMode, float tickTime = 1.0f / 60.0f) :
        base(GameWindowSettings.Default, new NativeWindowSettings() { Title = title, WindowState = WindowState.Maximized})
    {
        _tickTime = Mathematics.Clamp(tickTime, 1.0f / 300.0f, 1.0f);
        _sceneManager.SetGameApplication(this);
        _windowAspectRatio = (float)ClientSize.X / ClientSize.Y;
        _editMode = editMode;

        GL.Viewport(0, 0, Size.X, Size.Y + 50);
    }

    public void UnloadScene() => _sceneManager.UnloadScene();
    public void SetBackgroundColor(Color color) => _renderSystem.BackgroundColor = color;

    public void SetVSync(bool enabled) => VSync = enabled ? VSyncMode.On : VSyncMode.Off;

    public void LoadScene(BaseScene scene)
    {
        _sceneManager.LoadScene(scene);
    }

    [STAThread]
    protected override void OnLoad()
    {
        base.OnLoad();

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _sceneManager.SetGameApplication(this);

        if (EditMode && _sceneManager.CurrentScene is not null)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _editorWindow = new EditorWindow(_sceneManager);
            _editorWindow.Show();
            _editorWindow?.InitGameObjects();
        }
    }

    protected override void OnUnload()
    {
        if (EditMode)
            _sceneManager.CurrentScene?.SerializeScene();

        _sceneManager.UnloadScene();
        _sceneManager.Cleanup();
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
        _fpsTime += deltaTime;
        _frameCount++;

        if (_fpsTime >= 1)
        {
            FPS = (int)MathF.Round(_frameCount / _fpsTime);

            Console.WriteLine($"FPS: {FPS}");

            _fpsTime = 0.0f;
            _frameCount = 0;
        }
#endif
        _accumulator += deltaTime;

        while (_accumulator >= _tickTime)
        {
            if (EditMode == false)
                _sceneManager.CurrentScene?.FixedUpdate(_tickTime);

            _accumulator -= _tickTime;
        }


        if (EditMode == false)
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

        if (EditMode == false)
            _sceneManager.CurrentScene?.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyboardKeyEventArgs e)
    {
        base.OnKeyUp(e);

        if (EditMode == false)
            _sceneManager.CurrentScene?.OnKeyUp(e);
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        if (EditMode == false)
            _sceneManager.CurrentScene?.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        if (EditMode == false)
            _sceneManager.CurrentScene?.OnMouseUp(e);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        if (EditMode == false)
            _sceneManager.CurrentScene?.OnMouseWheel(e);
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        base.OnMouseMove(e);

        if (EditMode == false)
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
