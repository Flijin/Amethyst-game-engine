
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System.Diagnostics;
using Game_Engine.Core.Render;
using Game_Engine.Core;

namespace Game_Engine
{
    internal class Window : GameWindow
    {
        public Window(int wight, int height, string title)
            : base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = new Vector2i(wight, height) })
        {
            ClientSize = new Vector2i(wight, height);
            Title = title;
            _renderCore = new();
            test = new(new(@"C:\Users\it_ge\Desktop\Okay.stl")) { Scale = new(0.1f, 0.1f, 0.1f) };
        }

        private Stopwatch _stopwatch = new();
        private RenderCore _renderCore;
        GameObjectBase3D test;
        public static Camera Cam;

        sealed protected override void OnLoad()
        {
            Cam = new(Size);

            base.OnLoad();

            GL.ClearColor(0.4f, 0.4f, 0.4f, 1f);
            RenderCore.LoadGameObjectInGPU(test);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            _stopwatch.Restart();
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);
            _renderCore.DrawGameObject(test, Cam);
            SwapBuffers();

            Debug.Print((1000f / (_stopwatch.Elapsed.Microseconds / 1000f)).ToString());
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            _renderCore.Dispose();
        }
    }
}
