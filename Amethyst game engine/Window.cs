using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using Amethyst_game_engine.Core;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Amethyst_game_engine;

public class Window : GameWindow
{
    private BaseScene? _scene;

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

    internal BaseScene? Scene
    {
        set
        {
            _scene?.Dispose();
            _scene = value;
        }
    }

    static Window() => SystemSettings.Init();

    public Window(int wight, int height, string title)
        : base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = new Vector2i(wight, height) })
    {
        CursorState = CursorState.Grabbed;
        ClientSize = new Vector2i(wight, height);
        Title = title;

        GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
    }

    public Window(string title)
        : this(SystemSettings.SCREEN_RESOLUTION.X, SystemSettings.SCREEN_RESOLUTION.Y, title) { }

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

        GL.Clear(ClearBufferMask.ColorBufferBit);
        _scene?.DrawScene();

        SwapBuffers();
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _scene?.Dispose();
    }

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
