using Game_Engine.Enums;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using static System.Net.Mime.MediaTypeNames;

namespace Game_Engine.Core.Render
{
    internal class RenderCore(Vector2i windowSize) : IDisposable
    {
        private bool _disposed;
        private readonly Vector2i _windowSize = windowSize;
        private readonly Shader _shader = new(@"C:\Users\it_ge\source\repos\Game Engine\Game Engine\Shaders\Shader.vert",
                                              @"C:\Users\it_ge\source\repos\Game Engine\Game Engine\Shaders\Shader.frag");

        ~RenderCore()
        {
            if (_disposed == false)
            {
                Program.ShowWindow(Program.WINDOW_DESCRIPTOR, Program.SW_SHOW);
                Console.Write("Не был использован метод Dispose утечка памяти GPU");
            }
        }
        public static void LoadGameObjectInGPU(GameObjectBase3D obj)
        {
            var vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, obj.Model.Vertices.Length * sizeof(float),
                                                    obj.Model.Vertices, BufferUsageHint.DynamicDraw);

            var vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            obj.Handle = vertexArrayObject;
        }

        public void DrawGameObject(GameObjectBase3D obj)
        {
            GL.BindVertexArray(obj.Handle);
            _shader.Use();

            var uniformLocation = GL.GetUniformLocation(_shader.Handle, "aColor");
            var colorIndex = 0;

            for (int i = 0; i < obj.Model.TrianglesCount; i++)
            {
                GL.Uniform3(uniformLocation, obj.Model.GetData(AttribTypes.Color, colorIndex++));
                GL.DrawArrays(PrimitiveType.Triangles, i * 3, 3);
            }
        }

        public virtual void Dispose(bool disposing)
        {
            _shader.Dispose();
            _disposed = true;
        }

        public void Dispose()
        {
            if (_disposed == false)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
