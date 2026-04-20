using Amethyst_game_engine.Core.Render.Settings;
using OpenTK.Graphics.OpenGL4;

namespace Amethyst_game_engine.Core.GameObjects.Components;

internal struct TextureOptions
{
    public Sampler Sampler { get; set; }
    public PixelInternalFormat InternalFormat { get; set; }
    public TextureUnit Unit { get; set; }
    public RenderSettings Key { get; set; }
}
