//#define DEBUG_MODE

namespace Amethyst_game_engine.Render;

internal static class ShadersPool
{
    private static readonly Dictionary<uint, Shader> _shaders = [];

    public static Shader GetShader(uint flags, uint shadingModel)
    {
        if (_shaders.TryGetValue(flags, out Shader? result))
        {
            return result;
        }
        else
        {
            Shader shader = new(flags, shadingModel);
            _shaders.Add(flags, shader);

#if DEBUG_MODE
            System.Diagnostics.Debug.WriteLine($"A shader was built, key: {Convert.ToString(flags, 2)}");
#endif

            return shader;
        }
    }

    public static void Dispose()
    {
        foreach (var shader in _shaders.Values)
        {
            shader.Dispose();
        }
    }
}
