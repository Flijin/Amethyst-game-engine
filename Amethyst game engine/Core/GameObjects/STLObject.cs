using Amethyst_game_engine.CameraModules;
using Amethyst_game_engine.Models.STLModule;
using Amethyst_game_engine.Render;
using OpenTK.Graphics.ES30;

namespace Amethyst_game_engine.Core.GameObjects;

public class STLObject(STLModel model, bool useCamera, RenderSettings settings) : GameObject(model, useCamera, settings)
{
    private readonly STLModel _model = model;

    public STLObject(STLModel model, bool useCamera) : this(model, useCamera, RenderSettings.All)
    {

    }
}
