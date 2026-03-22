using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Amethyst_game_engine.Core;

namespace Amethyst_game_engine.Models.New_classes;

internal sealed class MeshData
{
    private unsafe float* _matrix;
    private readonly PrimitiveData[] _primitives;

    public PrimitiveData[] Primitives => _primitives;

    public unsafe float* Matrix
    {
        get => _matrix;
        set => _matrix = value;
    }

    public MeshData(PrimitiveData[] primitives)
    {
        _primitives = primitives;
    }
}
