namespace Amethyst_game_engine.Render;

internal static class ShadersCollection
{
    private static readonly Dictionary<int, Shader> _shaders = [];

    public static Shader GetShader(int flags)
    {
        if (_shaders.TryGetValue(flags, out Shader? result))
        {
            return result;
        }
        else
        {
            Shader shader = new(flags);
            _shaders.Add(flags, shader);
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
