using Amethyst_game_engine.Core;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Amethyst_game_engine.Models;

internal unsafe struct Mesh(Primitive[] primitives, int[] buffers) : IDisposable
{
    public readonly Primitive[] primitives = primitives;
    private readonly int[] _buffers = buffers;
    private float* _matrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);

    public required unsafe float* Matrix
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
                Unsafe.InitBlock(_matrix, 0, Mathematics.MATRIX_SIZE);

                _matrix[0] = 1;
                _matrix[5] = 1;
                _matrix[10] = 1;
                _matrix[15] = 1;
            }
        }
    }

    public readonly void Dispose()
    {
        foreach (var buffer in _buffers)
            GL.DeleteBuffer(buffer);

        foreach (var primitive in primitives)
            GL.DeleteVertexArray(primitive.vao);

        Marshal.FreeHGlobal((nint)_matrix);
    }
}
