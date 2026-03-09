using System.Diagnostics.CodeAnalysis;
using Amethyst_game_engine.Render;

namespace Amethyst_game_engine.Models.New_classes;

public class STLModel
{
    private readonly MeshData _meshData;
    private readonly RenderSettings _settings;

    public RenderSettings Settings => _settings;
    internal MeshData MeshData => _meshData;
    public int TrianglesCount { get; internal set; }

    [AllowNull]
    public string Header { get; internal set; }

    internal STLModel(MeshData mesh, RenderSettings settings)
    {
         _meshData = mesh;
        _settings = settings;
    }
}
