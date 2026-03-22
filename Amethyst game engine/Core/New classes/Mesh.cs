using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Amethyst_game_engine.Render;

namespace Amethyst_game_engine.Core.New_classes;

internal class Mesh : IDisposable
{
    private readonly Primitive[] _primitives;
    private unsafe float* _matrix;
    private readonly bool _useMeshMatrix;

    public Primitive[] Primitives => _primitives;

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

    public bool UseMeshMatrix => _useMeshMatrix;

    public Mesh(Primitive[] primitives, bool useMeshMatrix)
    {
        _primitives = primitives;
        _useMeshMatrix = useMeshMatrix;

        if (useMeshMatrix)
        {
            unsafe { _matrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE); }
        }
    }

    public void BuildShaders(ShaderBuildingProps props, RenderSettings global)
    {
        foreach (var primitive in _primitives)
            primitive.BuildShader(props, global);
    }

    public void UpdateShaders(ShaderBuildingProps props)
    {
        foreach (var primitive in _primitives)
            primitive.UpdateShader(props);
    }

    public void Dispose()
    {
        unsafe { Marshal.FreeHGlobal((nint)_matrix); }

        foreach (var primitive in _primitives)
            primitive.Dispose();
    }
}
