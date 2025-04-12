using OpenTK.Graphics.ES30;

namespace Amethyst_game_engine.Render;

internal class ShaderDataTransmitter
{
    private static readonly int[] _lastBindedTexture = [..Enumerable.Repeat(-1, 4)];

    public static void BindTexturesToUnits(Dictionary<TextureUnit, int> param)
    {
        if (param.Count == 0)
            return;

        var keyValuePairs = param.ToArray();

        for (int i = 0; i < keyValuePairs.Length; i++)
        {
            var textureUnit = keyValuePairs[i].Key;
            var textureHandler = keyValuePairs[i].Value;
            var numberOfUnit = (int)textureUnit - (int)TextureUnit.Texture0;

            if (_lastBindedTexture[numberOfUnit] != textureHandler)
            {
                GL.ActiveTexture(textureUnit);
                GL.BindTexture(TextureTarget.Texture2D, textureHandler);

                _lastBindedTexture[numberOfUnit] = textureHandler;
            }
        }
    }
}
