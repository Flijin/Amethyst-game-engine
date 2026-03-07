using Amethyst_game_engine.Core.New_classes;
using Amethyst_game_engine.Render;

namespace Amethyst_game_engine.Models.STLModule;

public readonly struct STLModel
{
    public STLModel(string path, RenderSettings renderSettings = RenderSettings.All, Material? material = null)
    {
        StreamReader readeer = new(path);
    }
}
