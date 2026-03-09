using Amethyst_game_engine.Render;
using OpenTK.Graphics.ES30;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Buffer = System.Buffer;

namespace Amethyst_game_engine.Core.New_classes;

internal class Mesh : IDisposable
{
    public readonly Primitive[] primitives;
    private readonly int[] _buffers;
    private readonly unsafe float* _matrix;

    public Mesh(Primitive[] primitives, int[] buffers)
    {
        this.primitives = primitives;
        _buffers = buffers;

        unsafe
        {
            _matrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);
        }
    }

    public unsafe float* Matrix
    {
        get => _matrix;

        set
        {
            if (value is not null)
            {
                Buffer.MemoryCopy(value, _matrix, Mathematics.MATRIX_SIZE, Mathematics.MATRIX_SIZE);
            }
            else
            {
                Unsafe.InitBlock(_matrix, 0, Mathematics.MATRIX_SIZE);

                _matrix[0] = _matrix[5] = _matrix[10] = _matrix[15] = 1;
            }
        }
    }

    public void BuildShader(ShaderBuildingProps props)
    {
        foreach (var primitive in primitives)
        {
            primitive.BuildShader(props);
        }
    }

    public void Dispose()
    {
        foreach (var buffer in _buffers)
            GL.DeleteBuffer(buffer);

        unsafe { Marshal.FreeHGlobal((nint)_matrix); }
    }
}
