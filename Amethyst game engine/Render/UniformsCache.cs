using System.Reflection;

namespace Amethyst_game_engine.Render;

internal static class UniformsCache
{
    public static readonly Dictionary<RenderSettings, string> uniforms = [];

    static UniformsCache()
    {
        Type enumType = typeof(RenderSettings);

        foreach (var setting in Enum.GetValues<RenderSettings>())
        {
            FieldInfo field = enumType.GetField(setting.ToString())!;
            UniformNameAttribute? fieldAttrib = field.GetCustomAttribute<UniformNameAttribute>();

            if (fieldAttrib != null)
                uniforms[setting] = fieldAttrib.Name;
        }
    }

    public static bool TryGetUniformName(RenderSettings settings, out string? name)
    {
        return uniforms.TryGetValue(settings, out name);
    }
}
