using Amethyst_game_engine.CameraModules;
using Amethyst_game_engine.Models.GLBModule;
using OpenTK.Graphics.ES20;
using System.Diagnostics.CodeAnalysis;

namespace Amethyst_game_engine.Core.GameObjects;

/*
 * Ooops, There are still bugs here
 */

[Obsolete("I've done some shit here, Dont use it")]
public class GLBObject : GameObject
{
    public GLBObject(GLBModel model, bool useCamera) : base(null, useCamera)
    {
        
    }
}
