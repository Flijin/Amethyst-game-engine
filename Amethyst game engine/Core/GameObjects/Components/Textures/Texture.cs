using Amethyst_game_engine.Core.Render;
using Amethyst_game_engine.Core.Render.Components;
using Amethyst_game_engine.Core.Render.Settings;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;

namespace Amethyst_game_engine.Core.GameObjects.Components.Textures;

internal sealed class Texture : IDisposable
{
    public readonly Guid id;
    public readonly int textureHandle;
    public readonly TextureUnit unit;
    public readonly RenderSettings textureSetting;

    public Vector2i Size { get; }

    public Texture(byte[] data, TextureParams parameters, TextureUnit unit, RenderSettings textureSetting)
    {
        this.unit = unit;
        this.textureSetting = textureSetting;

        textureHandle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, textureHandle);

        id = Guid.NewGuid();

        ImageResult image = ImageResult.FromMemory(data, parameters.Componets);

        Size = new(image.Width, image.Height);

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

    public void UseTexture(Shader shader) => TextureActivator.UseTexture(this, shader);

    public void Dispose()
    {
        GL.DeleteTexture(textureHandle);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }
}
