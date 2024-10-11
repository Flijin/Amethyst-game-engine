using OpenTK.Mathematics;
using Game_Engine.Core.CameraModules;
using Game_Engine.Core;

namespace Game_Engine;

internal class TestScene : BaseScene
{
    public TestScene(Vector2i windowSize) : base(windowSize)
    {
        AddCamera(new Vector3(0f, 0f, 50f), 45f, "Main");

        AddGameObject(new StaticGameObject3D(new STLModel(@"C:\\Users\\it_ge\\Desktop\\Okay.stl"))
        {
            Scale = new(0.01f, 0.01f, 0.01f)
        });

        AddControllerToCamera(new StandartCameraController(25f, 0.05f), "Main");
    }
}
