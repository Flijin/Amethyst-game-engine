using Amethyst_game_engine.Core.Utilities;
using OpenTK.Graphics.OpenGL4;

namespace Amethyst_game_engine.Core.Render;

internal class RenderSystem
{
    private Color _backgroundColor;
    private bool _clearBackground = true;

    public Color BackgroundColor
    {
        get => _backgroundColor;
        set => _backgroundColor = value;
    }

    public bool ClearBackground
    {
        get => _clearBackground;
        set => _clearBackground = value;
    }

    public RenderSystem() => _backgroundColor = new Color(127, 127, 127);

    public void Render(BaseScene scene)
    {
        if (_clearBackground)
        {
            GL.ClearColor(_backgroundColor.r, _backgroundColor.g, _backgroundColor.b, _backgroundColor.a);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        GL.Clear(ClearBufferMask.DepthBufferBit);

        foreach (var gameobj in scene.GameObjectManager.GameObjects)
        {
            if (gameobj.Visible)
                gameobj.DrawObject();
        }
    }
}
