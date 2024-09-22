
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;

namespace Game_Engine
{
    internal class Window : GameWindow
    {
        private readonly float[] _vertices;
        private readonly Shader _shader;
        private int _vertexBufferObject;
        private int _vertexArrayObject;

        public Window(int wight, int height, string title) :
            base(GameWindowSettings.Default, new NativeWindowSettings()
            {
                ClientSize = new Vector2i(wight, height),
                Title = title        
            })
        {
            _vertices = [ -1.0f, -1.0f, -1.0f,
                          0.0f, 1.0f, 0.0f,
                          1.0f, -1.0f, 0.0f ];

            _shader = new Shader(@"C:\Users\it_ge\source\repos\Game Engine\Game Engine\Shaders\Shader.vert",
                                 @"C:\Users\it_ge\source\repos\Game Engine\Game Engine\Shaders\Shader.frag");
        }

        sealed protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.7f, 0.6f, 0.2f, 1f);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            _shader.Use();
            GL.BindVertexArray(_vertexArrayObject);
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
