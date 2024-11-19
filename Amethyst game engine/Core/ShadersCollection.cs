using Amethyst_game_engine.Render;

namespace Amethyst_game_engine.Core;

internal static class ShadersCollection
{
    public static Dictionary<int, Shader> shaders = [];

    public static void InitShaders()
    {
        shaders[0] = new Shader(0);
        //shaders[1] = new Shader(1);
    }

    public static void Dispose()
    {
        shaders[0].Dispose();
        //shaders[1].Dispose();
    }
}
