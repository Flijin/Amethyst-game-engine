
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
            //CursorState = CursorState.Grabbed;
            ClientSize = new Vector2i(wight, height);
            Title = title;

            _test = new(Size);
        }

        private Vector2 _previewMousePos;
        private bool _isFirstMouseMove = true;
        private TestScene _test;

        sealed protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(_test.BackgroundColor.X, _test.BackgroundColor.Y, _test.BackgroundColor.Z, 1f);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);
            _test.DrawScene();

            SwapBuffers();
        }

        protected override void OnUnload()
        {
            base.OnUnload();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (IsFocused == false)
                return;

            var inputKey = KeyboardState;

            //if (inputKey.IsKeyDown(Keys.W)) cam.Position += cam.Front * 50f * (float)args.Time;
            //if (inputKey.IsKeyDown(Keys.S)) cam.Position -= cam.Front * 50f * (float)args.Time;
            //if (inputKey.IsKeyDown(Keys.A)) cam.Position -= Vector3.Normalize(Vector3.Cross(cam.Front, Vector3.UnitY)) * 50f * (float)args.Time;
            //if (inputKey.IsKeyDown(Keys.D)) cam.Position += Vector3.Normalize(Vector3.Cross(cam.Front, Vector3.UnitY)) * 50f * (float)args.Time;
            //if (inputKey.IsKeyDown(Keys.Space)) cam.Position += Vector3.UnitY * 50f * (float)args.Time;
            //if (inputKey.IsKeyDown(Keys.LeftShift)) cam.Position -= Vector3.UnitY * 50f * (float)args.Time;
            //if (inputKey.IsKeyDown(Keys.Escape))
            //{
            //    CursorState = CursorState.Normal;
            //    _isFirstMouseMove = true;
            //}
        }

        protected override void OnMouseMove(MouseMoveEventArgs args)
        {
            base.OnMouseMove(args);

            //if (_isFirstMouseMove == true && CursorState != CursorState.Normal)
            //{
            //    _isFirstMouseMove = false;
            //    _previewMousePos = args.Position;
            //}
            //else if (CursorState != CursorState.Normal)
            //{
            //    var delta = _previewMousePos - args.Position;
            //    _previewMousePos = args.Position;

            //    cam.Yaw += delta.X * 0.001f;
            //    cam.Pitch += delta.Y * 0.001f;
            //}
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            //CursorState = CursorState.Grabbed;
        }
    }
}
