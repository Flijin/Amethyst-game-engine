using Amethyst_game_engine.Models.Components;

namespace Amethyst_game_engine.Models.GLBModule;

public class GLBModel
{
    private readonly List<MeshData> _meshesData;
    internal List<MeshData> MeshesData => _meshesData;

    internal GLBModel(List<MeshData> mesh)
    {
        _meshesData = mesh;
    }
}
