using System.Diagnostics.CodeAnalysis;
using Amethyst_game_engine.Render;
using OpenTK.Graphics.OpenGL4;

namespace Amethyst_game_engine.Core;

internal class TextureActivator
{
    [AllowNull]
    private static readonly Dictionary<TextureUnit, Texture> _lastTextures = [];

    private static readonly Dictionary<TextureUnit, string> _samplers = new()
    {
        [TextureUnit.Texture0] = "_albedoTexture",
        [TextureUnit.Texture1] = "_emissiveTexture",
        [TextureUnit.Texture2] = "_normalTexture",
        [TextureUnit.Texture3] = "_occlusionTexture",
        [TextureUnit.Texture4] = "_metallicRoughnessTexture",
    };

    public static void ResetTexture() => _lastTextures.Clear();

    public static void ActivateTexture(Texture texture, Shader shader, TextureUnit unit)
    {
        if (_lastTextures.TryGetValue(unit, out Texture? value) && value.id == texture.id)
            return;

        _lastTextures[unit] = texture;
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2D, texture.textureHandle);

        shader.SetInt(_samplers[unit], (int)unit - (int)TextureUnit.Texture0);
    }
}
