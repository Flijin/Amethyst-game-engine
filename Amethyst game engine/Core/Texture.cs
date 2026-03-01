using Amethyst_game_engine.Render;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace Amethyst_game_engine.Core;

internal class Texture
{
    public readonly Guid id;
    public readonly int textureHandle;
    public readonly TextureUnit unit;

    public Texture(byte[] data, TextureParams parameters, TextureUnit unit)
    {
        this.unit = unit;

        textureHandle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, textureHandle);

        id = Guid.NewGuid();

        ImageResult image = ImageResult.FromMemory(data, parameters.Componets);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, parameters.WrapS);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, parameters.WrapT);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, parameters.MagFilter);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, parameters.MinFilter);

        GL.TexImage2D(TextureTarget.Texture2D, 0,
                      parameters.PixelInternalFormat,
                      image.Width, image.Height, 0,
                      parameters.PixelFormat, PixelType.UnsignedByte,
                      image.Data);

        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void UseTexture(Shader shader, TextureUnit unit) => TextureActivator.UseTexture(this, shader);
}
