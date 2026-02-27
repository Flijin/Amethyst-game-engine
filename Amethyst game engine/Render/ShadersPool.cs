//#define DEBUG_MODE

namespace Amethyst_game_engine.Render;

internal static class ShadersPool
{
    private static readonly Dictionary<ShaderBuildingProps, Shader> _shaders = [];

    public static Shader GetShader(ShaderBuildingProps props)
    {
        if (_shaders.TryGetValue(props, out Shader? result))
        {
            return result;
        }
        else
        {
            Shader shader = new(props);
            _shaders.Add(props, shader);

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
