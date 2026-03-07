using OpenTK.Graphics.OpenGL4;

namespace Amethyst_game_engine.Core.New_classes;

internal class RenderSystem
{
    private Color _backgroundColor;

    public Color BackgroundColor
    {
        get => _backgroundColor;

        set
        {
            _backgroundColor = value;
            GL.ClearColor(_backgroundColor.r, _backgroundColor.g, _backgroundColor.b, 1.0f);
        }
    }
    public RenderSystem() => BackgroundColor = new Color(127, 127, 127);

    public void Render(IBaseScene scene)
    {
        GL.ClearColor(_backgroundColor.r, _backgroundColor.g, _backgroundColor.b, _backgroundColor.a);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        foreach (var gameobj in scene.GameObjectManager.GameObjects)
        {
            gameobj.DrawObject();
        }
    }
}
