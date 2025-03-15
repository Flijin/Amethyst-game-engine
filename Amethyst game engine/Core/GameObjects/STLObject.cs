using Amethyst_game_engine.CameraModules;
using Amethyst_game_engine.Models;
using Amethyst_game_engine.Models.STLModule;
using Amethyst_game_engine.Render;
using OpenTK.Graphics.ES30;

namespace Amethyst_game_engine.Core.GameObjects;

public class STLObject : GameObject
{
    private Primitive _objectPrimitive;
    private readonly STLModel _model;

    public Material Material
    {
        get => _objectPrimitive.Material;

        set
        {
            _objectPrimitive.Material = value;
            _objectPrimitive.BuildShader(value.materialKey & _currentRenderState & (uint)Window.RenderKeys, 0);
        }
    }

    public STLObject(STLModel model, bool useCamera) : base(model, useCamera)
    {
        _objectPrimitive = ((IModel)model).GetMeshes()[0].primitives[0];
        _model = model;
    }

    public STLObject(STLModel model, bool useCamera, RenderSettings settings) : base(model, useCamera, settings)
    {
        
    }
}
