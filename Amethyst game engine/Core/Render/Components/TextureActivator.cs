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

    public static void ResetTextures() => _lastTextures.Clear();

    public static void UseTexture(Texture texture, Shader shader)
    {
        if (_lastTextures.TryGetValue(texture.Unit, out Texture? value) && value == texture)
            return;

        _lastTextures[texture.Unit] = texture;
        GL.ActiveTexture(texture.Unit);
        GL.BindTexture(TextureTarget.Texture2D, texture.TextureHandle);

        shader.SetInt(_samplers[texture.Unit], (int)texture.Unit - (int)TextureUnit.Texture0);
    }
}
