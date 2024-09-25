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

        public void LoadModelInGPU(STLModel model)
        {
            var vertices = new float[model.TrianglesCount * 5];
            var index = 0;

            foreach (var polygon in model.Triangles)
            {
                foreach (var vertex in polygon.Vertices)
                {
                    foreach (var coordinate in GetVectorCoordonates(NormalizeVector(vertex)))
                    {
                        vertices[index++] = coordinate;
                    }
                }
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

        private Vector3 NormalizeVector(Vector3 vector) => vector / new Vector3(_windowSize.X, _windowSize.Y, _windowSize.X);

        private static IEnumerable<float> GetVectorCoordonates(Vector3 vector)
        {
            yield return vector.X;
            yield return vector.Y;
            yield return vector.Z;
        }
    }
}
