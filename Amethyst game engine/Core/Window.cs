using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Amethyst_game_engine.Render;
using System.ComponentModel;
using System.Diagnostics;

namespace Amethyst_game_engine.Core;

public class Window : GameWindow
{
    private static BaseScene? _scene;
    private static float _aspectRatio;
    private static RenderSettings _renderSettings = RenderSettings.All;

    private static Action<KeyboardState, float>? _keyPressedHandler;
    internal static event Action<KeyboardState, float> KeyPressedEvent
    {
        add => _keyPressedHandler += value;
        remove => _keyPressedHandler -= value;
    }

    private static Action<Vector2>? _mouseMoveHandler;
    internal static event Action<Vector2> MouseMoveEvent
    {
        add => _mouseMoveHandler += value;
        remove => _mouseMoveHandler -= value;
    }

    private static Action? _resetFirstMoveHadler;
    internal static event Action ResetFirstMoveEvent
    {
        add => _resetFirstMoveHadler += value;
        remove => _resetFirstMoveHadler -= value;
    }

    internal static new float AspectRatio => _aspectRatio;
    public static float DeltaTime { get; private set; }

    public static RenderSettings RenderKeys
    {
        get => _renderSettings;

        set
        {
            _scene?.ChangeGlobalRenderSettings((uint)value);
            _renderSettings = value;
        }
    }

    public static BaseScene? Scene
    {
        set
        {
            _scene?.Dispose();
            _scene = value;
        }
    }

    static Window() => SystemSettings.Init();

    public Window(int width, int height, string title)
        : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            ClientSize = new Vector2i(width, height),
        })
    {
        SystemSettings.ShowWindow(SystemSettings.SW_HIDE);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _aspectRatio = (float)width / height;
        CursorState = CursorState.Grabbed;
        ClientSize = new Vector2i(width, height);
        Title = title;

        GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
    }

    public Window(string title)
        : this(SystemSettings.ScreenResolution.X, SystemSettings.ScreenResolution.Y, title) { }

    public static void ChangeBackgroundColor(Vector3 color) => GL.ClearColor(color.X, color.Y, color.Z, 1.0f);

    internal static void ClearInputHandlers()
    {
        _keyPressedHandler = null;
        _mouseMoveHandler = null;
        _resetFirstMoveHadler = null;
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _scene?.DrawScene();
        DeltaTime = (float)args.Time;

        SwapBuffers();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        Debug.WriteLine("Работает OnClosing()");
        ShadersPool.Dispose();
        //_scene?.Dispose();
        Debug.WriteLine("Все очистилось");
    }

    //protected override void OnUnload()
    //{
    //    Debug.WriteLine("Работает OnUnload()");
    //    _scene?.Dispose();
    //    ShadersPool.Dispose();
    //    Debug.WriteLine("Все очистилось");
    //    base.OnUnload();
    //}

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (IsFocused == false)
            return;

        var inputKey = KeyboardState;
        _keyPressedHandler?.Invoke(inputKey, (float)args.Time);

        if (inputKey.IsKeyDown(Keys.Escape))
        {
            CursorState = CursorState.Normal;
            _resetFirstMoveHadler?.Invoke();
        }
    }

    protected override void OnMouseMove(MouseMoveEventArgs args)
    {
        base.OnMouseMove(args);

        if (CursorState != CursorState.Normal)
        {
            _mouseMoveHandler?.Invoke(args.Position);
        }
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        CursorState = CursorState.Grabbed;
    }
}
