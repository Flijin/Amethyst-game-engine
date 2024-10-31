using Amethyst_game_engine.CameraModules;
using Amethyst_game_engine.Core;
using Amethyst_game_engine.Models.STLModule;
using OpenTK.Mathematics;

namespace Client;

internal class TestScene : BaseScene
{
    public TestScene() : base()
    {
        AddCamera(new Camera(CameraTypes.Perspective, new Vector3(0, 0, 200), AspectRatio), "MainCamera");

        AddGameObject(new StaticGameObject3D(new STLModel(@"C:\Users\it_ge\Desktop\Okay.stl"))
                                            { Scale = new Vector3(0.01f, 0.01f, 0.01f) });

        AddControllerToCamera(new StandartCameraController(50f, 0.01f), "MainCamera");

        
    }
}
