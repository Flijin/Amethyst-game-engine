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
            if (_lastBindedTexture[(int)keyValuePairs[i].Key - 33984] != keyValuePairs[i].Value)
            {
                GL.ActiveTexture(keyValuePairs[i].Key);
                GL.BindTexture(TextureTarget.Texture2D, keyValuePairs[i].Value);

                _lastBindedTexture[i] = keyValuePairs[i].Value;
            }
        }
    }
}
