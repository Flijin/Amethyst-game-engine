using Amethyst_game_engine.Core.GameObjects.Components;
using Amethyst_game_engine.Models.Components;
using OpenTK.Mathematics;
using System.Diagnostics.CodeAnalysis;

namespace Amethyst_game_engine.Models.GLBModule;

public class GLBModel
{
    private readonly List<MeshData> _meshesData;
    internal List<MeshData> MeshesData => _meshesData;

    internal BoundingBox Box { get; }

    [AllowNull]
    internal string Path { get; set; }
    internal int GLBModelIndex { get; set; }

    internal GLBModel(List<MeshData> meshes)
    {
        _meshesData = meshes;

        Vector3 min = new(float.MaxValue);
        Vector3 max = new(float.MinValue);

        foreach (var mesh in meshes)
        {
            min = Vector3.ComponentMin(min, mesh.Box.Min);
            max = Vector3.ComponentMax(max, mesh.Box.Max);
        }

        Box = new(min, max);
    }
}
