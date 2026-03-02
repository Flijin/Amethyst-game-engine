using System.Reflection;

namespace Amethyst_game_engine.Render;

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

            _renderSettingsCache[currentFlag] = field.GetCustomAttribute<GLSLMacrosAttribute>()!.Macro;

            currentFlag <<= 1;
        }
    }

    private static void InitShadingModelCache()
    {
        byte currentFlag = 0;
        byte maxValue = (byte)ShadingModels.PBR_MetallicRoughness;

        while (currentFlag <= maxValue)
        {
            FieldInfo field = typeof(ShadingModels).GetField(((ShadingModels)currentFlag).ToString())!;

            _shadingModelCache[currentFlag] = field.GetCustomAttribute<GLSLMacrosAttribute>()!.Macro;

            currentFlag++;
        }
    }
}
