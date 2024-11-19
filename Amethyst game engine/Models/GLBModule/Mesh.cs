using OpenTK.Graphics.OpenGL4;

namespace Amethyst_game_engine.Models.GLBModule;

internal struct Mesh(Primitive[] primitives, int[] buffers) : IDisposable
{
    public readonly Primitive[] primitives = primitives;
    private readonly int[] _buffers = buffers;
    private float[,] _matrix;

    public required float[,] Matrix
    {
        readonly get => _matrix;

        set
        {
            if (value is not null)
            {
                _matrix = value;
            }
            else
            {
                _matrix = new float[4, 4]
                {
                    { 1, 0, 0, 0 },
                    { 0, 1, 0, 0 },
                    { 0, 0, 1, 0 },
                    { 0, 0, 0, 1 }
                };
            }
        }
    }
    
    public readonly void Dispose()
    {
        foreach (var buffer in _buffers)
            GL.DeleteBuffer(buffer);

        foreach (var primitive in primitives)
            GL.DeleteVertexArray(primitive.vao);
    }
}
