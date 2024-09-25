
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System.Diagnostics;
using Game_Engine.Core.Render;

namespace Game_Engine
{
    internal class Window(int wight, int height, string title) : GameWindow(GameWindowSettings.Default, new NativeWindowSettings()
        {
            ClientSize = new Vector2i(wight, height),
            Title = title        
        })
    {

        private readonly Shader _shader = new(@"C:\Users\it_ge\source\repos\Game Engine\Game Engine\Shaders\Shader.vert",
                                                    @"C:\Users\it_ge\source\repos\Game Engine\Game Engine\Shaders\Shader.frag");
        private float[] _vertices = [ -1.0f, -1.0f, 1.0f, 0.0f, 0.0f,
                           0.0f,  1.0f, 0.0f, 1.0f, 0.0f,
                           1.0f, -1.0f, 0.0f, 0.0f, 1.0f, ];
        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private Stopwatch _stopwatch = new();

        sealed protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.4f, 0.4f, 0.4f, 1f);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            _stopwatch.Restart();
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            var sin = (float)Math.Abs(Math.Sin(_stopwatch.Elapsed.TotalSeconds));
            var cos = (float)Math.Abs(Math.Cos(_stopwatch.Elapsed.TotalSeconds));

            _vertices = [ -1.0f, -1.0f, cos, sin, 0.0f,
                           0.0f,  1.0f, 0.0f, cos, sin,
                           1.0f, -1.0f, sin, 0.0f, cos, ];

            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _shader.Use();
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            SwapBuffers();
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            _shader.Dispose();
        }
    }
}
