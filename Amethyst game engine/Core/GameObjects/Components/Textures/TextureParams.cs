using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace Amethyst_game_engine.Core.GameObjects.Components.Textures;

internal class TextureParams
{
    public PixelInternalFormat PixelInternalFormat { get; set; }
    public PixelFormat PixelFormat { get; set; }
    public ColorComponents Componets { get; set; }
    public int MyProperty { get; set; }
    public int WrapS { get; set; }
    public int WrapT { get; set; }
    public int MagFilter { get; set; }
    public int MinFilter { get; set; }
}
