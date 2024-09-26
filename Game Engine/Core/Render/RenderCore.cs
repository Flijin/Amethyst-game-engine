using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

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

        public void UseShader() => _shader.Use();

        public void LoadModelInGPU(STLModel model)
        {
            var vertices = new float[model.TrianglesCount * 18];
            var index = 0;
            Random r = new();

            foreach (var polygon in model.Triangles)
            {
                foreach (var vertex in polygon.Vertices)
                {
                    foreach (var coordinate in GetVectorCoordonates(NormalizeVector(vertex)))
                    {
                        vertices[index++] = coordinate;
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        vertices[index++] = (float)r.NextDouble();
                    }
                }
            }

            var vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            var vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
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

        private Vector3 NormalizeVector(Vector3 vector) => vector / new Vector3(_windowSize.X, _windowSize.Y, _windowSize.X);

        private static IEnumerable<float> GetVectorCoordonates(Vector3 vector)
        {
            yield return vector.X;
            yield return vector.Y;
            yield return vector.Z;
        }
    }
}
