using Amethyst_game_engine.CameraModules;
using Amethyst_game_engine.Models.GLBModule;
using OpenTK.Graphics.ES30;
using System.Diagnostics.CodeAnalysis;

namespace Amethyst_game_engine.Core.GameObjects;

public class GLBObject : GameObject
{
    public GLBObject(GLBModel model, bool useCamera) : base(model, useCamera)
    {
        
    }
}
