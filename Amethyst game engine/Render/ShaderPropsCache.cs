using System.Reflection;

namespace Amethyst_game_engine.Render;

internal static class ShaderPropsCache
{
    public static readonly Dictionary<uint, string> renderSettingsCache = [];
    public static readonly Dictionary<byte, string> shadingModelCache = [];
    public static readonly Dictionary<uint, string> specialSettingsCache = [];

    static ShaderPropsCache()
    {
        InitRenderSettingsCache();
        InitShadingModelCache();
        InitSpecialSettingsCache();
    }

    private static void InitRenderSettingsCache()
    {
        uint currentFlag = 1;
        var maxValue = (uint)RenderSettings.All;

        while (currentFlag < maxValue)
        {
            FieldInfo field = typeof(RenderSettings).GetField(((RenderSettings)currentFlag).ToString())!;

            renderSettingsCache[currentFlag] = field.GetCustomAttribute<GLSLMacrosAttribute>()!.Macro;

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

            shadingModelCache[currentFlag] = field.GetCustomAttribute<GLSLMacrosAttribute>()!.Macro;

            currentFlag++;
        }
    }

    private static void InitSpecialSettingsCache()
    {
        uint currentFlag = 1;
        var maxValue = (uint)SpecialSettings.UseMeshMatrix;

        while (currentFlag <= maxValue)
        {
            FieldInfo field = typeof(SpecialSettings).GetField(((SpecialSettings)currentFlag).ToString())!;

            specialSettingsCache[currentFlag] = field.GetCustomAttribute<GLSLMacrosAttribute>()!.Macro;

            currentFlag <<= 1;
        }
    }
}
