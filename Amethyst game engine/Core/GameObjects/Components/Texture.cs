using Amethyst_game_engine.Core.Render;
using Amethyst_game_engine.Core.Render.Components;
using Amethyst_game_engine.Core.Render.Settings;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Amethyst_game_engine.Core.GameObjects.Components;

internal sealed class Texture : IDisposable
{
    public readonly Guid id;
    public readonly int textureHandle;
    public readonly TextureUnit unit;
    public readonly RenderSettings key;

    public Texture(byte[] data, TextureOptions options, TextureUnit unit, RenderSettings key)
    {
        this.unit = unit;
        this.key = key;

        textureHandle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, textureHandle);

        id = Guid.NewGuid();

        using var image = Image.Load<Rgba32>(data);

        byte[] pixels = new byte[image.Width * image.Height * 4];
        image.CopyPixelDataTo(pixels);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, options.Sampler.WrapS);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, options.Sampler.WrapT);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, options.Sampler.MagFilter);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, options.Sampler.MinFilter);

        GL.TexImage2D(TextureTarget.Texture2D, 0,
                      options.InternalFormat,
                      image.Width, image.Height, 0,
                      options.PixelFormat, PixelType.UnsignedByte,
                      pixels);

        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void UseTexture(Shader shader) => TextureActivator.UseTexture(this, shader);

    public void Dispose()
    {
        GL.DeleteTexture(textureHandle);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }
}
