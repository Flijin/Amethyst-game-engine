
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System.Diagnostics;
using Game_Engine.Core.Render;
using Game_Engine.Core;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Game_Engine
{
    internal class Window : GameWindow
    {
        public Window(int wight, int height, string title)
            : base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = new Vector2i(wight, height) })
        {
            CursorState = CursorState.Grabbed;
            ClientSize = new Vector2i(wight, height);
            Title = title;

            _renderCore = new();
            test = new(new(@"C:\Users\it_ge\Desktop\Okay.stl")) { Scale = new(0.1f, 0.1f, 0.1f) };
            cam = new(new Vector3(0f, 0f, 200f), Size, 45f);
        }

        private readonly Stopwatch _stopwatch = new();
        private readonly RenderCore _renderCore;
        private readonly GameObjectBase3D test;
        private readonly Camera cam;
        private Vector2 _previewMousePos;
        private bool _isFirstMouseMove = true;

        sealed protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.4f, 0.4f, 0.4f, 1f);
            RenderCore.LoadGameObjectInGPU(test);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            _stopwatch.Restart();
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);
            _renderCore.DrawGameObject(test, cam);
            SwapBuffers();

            //Debug.Print((1000f / (_stopwatch.Elapsed.Microseconds / 1000f)).ToString());
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            _renderCore.Dispose();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (IsFocused == false)
                return;

            var inputKey = KeyboardState;

            if (inputKey.IsKeyDown(Keys.W)) cam.Position += cam.Front * 50f * (float)args.Time;
            if (inputKey.IsKeyDown(Keys.S)) cam.Position -= cam.Front * 50f * (float)args.Time;
            if (inputKey.IsKeyDown(Keys.A)) cam.Position -= Vector3.Normalize(Vector3.Cross(cam.Front, Vector3.UnitY)) * 50f * (float)args.Time;
            if (inputKey.IsKeyDown(Keys.D)) cam.Position += Vector3.Normalize(Vector3.Cross(cam.Front, Vector3.UnitY)) * 50f * (float)args.Time;
            if (inputKey.IsKeyDown(Keys.Space)) cam.Position += Vector3.UnitY * 50f * (float)args.Time;
            if (inputKey.IsKeyDown(Keys.LeftShift)) cam.Position -= Vector3.UnitY * 50f * (float)args.Time;
            if (inputKey.IsKeyDown(Keys.Escape))
            {
                CursorState = CursorState.Normal;
                _isFirstMouseMove = true;
            }
        }

        protected override void OnMouseMove(MouseMoveEventArgs args)
        {
            base.OnMouseMove(args);

            if (_isFirstMouseMove == true && CursorState != CursorState.Normal)
            {
                _isFirstMouseMove = false;
                _previewMousePos = args.Position;
            }
            else if (CursorState != CursorState.Normal)
            {
                var delta = _previewMousePos - args.Position;
                _previewMousePos = args.Position;

                cam.Yaw += delta.X * 0.001f;
                cam.Pitch += delta.Y * 0.001f;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            CursorState = CursorState.Grabbed;
        }
    }
}
