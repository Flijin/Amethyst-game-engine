using OpenTK.Graphics.ES30;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Amethyst_game_engine.Render;

[Experimental("Needs_tests")]
internal class ShaderDataTransmitter
{
    private static readonly int[] _lastBindedTexture = [..Enumerable.Repeat(-1, 8)];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void BindTexturesToUnits(TextureUnit[] units, int[] textureHandlers)
    {
        for (int i = 0; i < units.Length; i++)
        {
            if (_lastBindedTexture[(int)units[i] - 33984] != textureHandlers[i])
            {
                GL.ActiveTexture(units[i]);
                GL.BindTexture(TextureTarget.Texture2D, textureHandlers[i]);

                _lastBindedTexture[i] = textureHandlers[i];
            }
        }
    }
}
