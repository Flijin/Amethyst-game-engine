using Amethyst_game_engine.CameraModule;
using OpenTK.Graphics.OpenGL4;

namespace Amethyst_game_engine.Core.New_classes;

internal sealed class RenderSystem
{
    private Color _backgroundColor;
    private List<MeshRenderer> _meshRenderers;

    public Color BackgroundColor
    {
        get => _backgroundColor;

        set
        {
            _backgroundColor = value;
            GL.ClearColor(_backgroundColor.r, _backgroundColor.g, _backgroundColor.b, 1.0f);
        }
    }
    public void Init()
    {
        BackgroundColor = new Color(127, 127, 127);
    }

    public void RegisterMeshRenderer(MeshRenderer renderer) => _meshRenderers.Add(renderer);
    public void UnRegisterMeshRenderer(MeshRenderer renderer) => _meshRenderers.Remove(renderer);

    public void Render(BaseScene scene)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        foreach (var gameobj in scene.GameObjectManager.GameObjects)
        {
            if (gameobj.UseCamera)
            {
                foreach (var camera in scene.CameraManager.Cameras)
                {
                    RenderMesh(camera);
                }
            }
            else
            {
                RenderMesh(null);
            }
        }
    }

    private void RenderMesh(Camera? cam)
    {

    }
}
