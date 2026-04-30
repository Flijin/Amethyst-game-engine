using Amethyst_game_engine.Core.GameObjects.Components;
using Amethyst_game_engine.Core.Utilities;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Models.Components;

internal sealed class MeshData
{
    private unsafe float* _matrix;
    private readonly PrimitiveData[] _primitives;

    public BoundingBox Box { get; private set; }

    public PrimitiveData[] Primitives => _primitives;

    public unsafe float* Matrix
    {
        get => _matrix;
        set
        {
            _matrix = value;

            if (value is not null)
                Box = BoundingBox.TransformBox(Box, value);
        }
    }

    internal MeshData(PrimitiveData[] primitives)
    {
        _primitives = primitives;

        Vector3 min = new(float.MaxValue);
        Vector3 max = new(float.MinValue);

        foreach (var primitive in primitives)
        {
            Vector3[] corners = primitive.Box.GetCorners();
            min = Vector3.ComponentMin(min, primitive.Box.Min);
            max = Vector3.ComponentMax(max, primitive.Box.Max);
        }

        Box = new BoundingBox(min, max);
    }
}
