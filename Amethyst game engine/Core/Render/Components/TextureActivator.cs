using Amethyst_game_engine.Core.GameObjects.Components;
using OpenTK.Graphics.OpenGL4;

namespace Amethyst_game_engine.Core.Render.Components;

internal class TextureActivator
{
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

    public static void UseTexture(Texture texture, Shader shader)
    {
        if (_lastTextures.TryGetValue(texture.unit, out Texture? value) && value.id == texture.id)
            return;

        _lastTextures[texture.unit] = texture;
        GL.ActiveTexture(texture.unit);
        GL.BindTexture(TextureTarget.Texture2D, texture.textureHandle);

        shader.SetInt(_samplers[texture.unit], (int)texture.unit - (int)TextureUnit.Texture0);
    }
}
