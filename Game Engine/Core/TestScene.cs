using OpenTK.Mathematics;

namespace Game_Engine.Core;

internal class TestScene : BaseScene
{
    public TestScene(Vector2i windowSize) : base(windowSize)
    {
        AddCamera(new Vector3(0f, 0f, 50f), 45f, "Main");

        AddGameObject(new GameObjectBase3D(new STLModel(@"C:\\Users\\it_ge\\Desktop\\Okay.stl"))
        {
            Scale = new(0.01f, 0.01f, 0.01f)
        });

        AddControllerToCamera(new StandartCameraController(25f, 0.05f), "Main");
    }
}
