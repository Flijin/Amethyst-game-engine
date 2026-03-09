using System.Diagnostics.CodeAnalysis;
using Amethyst_game_engine.Models.New_classes;
using Amethyst_game_engine.Render;

namespace Amethyst_game_engine.Core.New_classes;

public class STLGameObject : DrawableObject
{
    private Mesh _mesh;

    public STLGameObject(STLModel model, RenderSettings objectSettings = RenderSettings.All)
    {
        UseMeshMatrix = false;
        BuildObject(model);
    }

    [MemberNotNull(nameof(_mesh))]
    private void BuildObject(STLModel model)
    {
        Mesh mesh = new(,);
    }
}
