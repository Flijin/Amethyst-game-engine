using System.Reflection;
using Amethyst_game_engine.Core.Render.Settings;

namespace Amethyst_game_engine.Core.Render.Components;

internal static class MacrosCache
{
    private static readonly Dictionary<uint, string> _renderSettingsCache = [];
    private static readonly Dictionary<byte, string> _shadingModelCache = [];

    public static IReadOnlyDictionary<uint, string> RenderSettingsCache => _renderSettingsCache;
    public static IReadOnlyDictionary<byte, string> ShadingModelCache => _shadingModelCache;

    static MacrosCache()
    {
        InitRenderSettingsCache();
        InitShadingModelCache();
    }

    private static void InitRenderSettingsCache()
    {
        uint currentFlag = 1;
        var maxValue = (uint)RenderSettings.All;

        while (currentFlag < maxValue)
        {
            FieldInfo field = typeof(RenderSettings).GetField(((RenderSettings)currentFlag).ToString())!;

            var attribute = field.GetCustomAttribute<GLSLMacrosAttribute>();

            if (attribute is not null)
                _renderSettingsCache[currentFlag] = attribute.Macro;

            currentFlag <<= 1;
        }
    }

    private static void InitShadingModelCache()
    {
        byte currentFlag = 0;
        byte maxValue = (byte)ShadingModels.Unlit;
        while (currentFlag <= maxValue)
        {
            FieldInfo field = typeof(ShadingModels).GetField(((ShadingModels)currentFlag).ToString())!;

            _shadingModelCache[currentFlag] = field.GetCustomAttribute<GLSLMacrosAttribute>()!.Macro;

            currentFlag++;
        }
    }
}
