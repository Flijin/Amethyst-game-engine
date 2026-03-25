
using Amethyst_game_engine.Core.Render.Settings;
using Amethyst_game_engine.Models.Components;

namespace Amethyst_game_engine.Models.GLBModule;

public class GLBModel
{
    private readonly MeshData _meshData;
    private readonly RenderSettings _settings;
    
    internal RenderSettings Settings => _settings;
    internal MeshData MeshData => _meshData;

    internal GLBModel(MeshData mesh, RenderSettings settings)
    {
        _meshData = mesh;
        _settings = settings;
    }
}
