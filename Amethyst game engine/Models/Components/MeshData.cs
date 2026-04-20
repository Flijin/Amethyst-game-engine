namespace Amethyst_game_engine.Models.Components;

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

    internal MeshData(PrimitiveData[] primitives)
    {
        _primitives = primitives;
    }
}
