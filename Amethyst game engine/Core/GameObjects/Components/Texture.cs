using System.Buffers;
using Amethyst_game_engine.Core.Render;
using Amethyst_game_engine.Core.Render.Components;
using Amethyst_game_engine.Core.Render.Settings;
using Amethyst_game_engine.Models.Components;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Amethyst_game_engine.Core.GameObjects.Components;

internal sealed class Texture : IDisposable
{
    public int TextureHandle { get; }
    public TextureUnit Unit { get; }
    public RenderSettings Key { get; }

    public unsafe Texture(TextureData texture)
    {
        Unit = texture.Options.Unit;
        Key = texture.Options.Key;

        TextureHandle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, TextureHandle);

        using var image = Image.Load<Rgba32>(texture.Data);

        int size = image.Width * image.Height * 4;
        byte[] pixels = ArrayPool<byte>.Shared.Rent(size);
        Span<byte> pixelsSpan = new(pixels, 0, size);

        image.CopyPixelDataTo(pixelsSpan);

        var minFilter = texture.Options.Sampler.MinFilter;

        var usesMipmaps = minFilter switch
        {
            9984 or 9985 or 9986 or 9987 => true,
            _ => false
        };

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, texture.Options.Sampler.WrapS);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, texture.Options.Sampler.WrapT);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, texture.Options.Sampler.MagFilter);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, minFilter);

        fixed (byte* pixelsPtr = pixelsSpan)
        {
            GL.TexImage2D(TextureTarget.Texture2D, 0,
              texture.Options.InternalFormat,
              image.Width, image.Height, 0,
              PixelFormat.Rgba, PixelType.UnsignedByte,
              (nint)pixelsPtr);
        }

        if (usesMipmaps)
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        GL.BindTexture(TextureTarget.Texture2D, 0);
        ArrayPool<byte>.Shared.Return(pixels);
    }

    public void UseTexture(Shader shader) => TextureActivator.UseTexture(this, shader);

    public void Dispose()
    {
        GL.DeleteTexture(TextureHandle);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }
}
