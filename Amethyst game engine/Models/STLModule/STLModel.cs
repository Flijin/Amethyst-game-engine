using System.Diagnostics.CodeAnalysis;
using Amethyst_game_engine.Core.GameObjects.Components;
using Amethyst_game_engine.Core.Render.Settings;
using Amethyst_game_engine.Models.Components;

namespace Amethyst_game_engine.Models.STLModule;

public class STLModel
{
    private readonly MeshData _meshData;
    private readonly RenderSettings _settings;

    internal BoundingBox Box { get; }

    internal RenderSettings Settings => _settings;
    internal MeshData MeshData => _meshData;
    public int TrianglesCount { get; internal set; }

    [AllowNull]
    public string Path { get; internal set;  }

    internal STLModel(MeshData mesh, RenderSettings settings)
    {
         _meshData = mesh;
        _settings = settings;

        Box = mesh.Box;
    }
}
