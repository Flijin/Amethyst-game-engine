using System.Text;

namespace Amethyst_game_engine.Render;

internal static class GLSLMacrosBuilder
{
    public static void BuildMacrosByFlags(ShaderBuildingProps props, StringBuilder builder)
    {
        IncludeRenderSetingsMacros(builder, props.RenderSettings);
        IncludeShadingModel(builder, props.ShadingModel);
        IncludeSpecialSettings(builder, props.SpecialSettings);
        IncludeGlobalSetttings(builder, props.GlobalSettings);
    }

    private static void IncludeRenderSetingsMacros(StringBuilder builder, RenderSettings settings)
    {
        uint currentFlag = 1;
        uint maxValue = (uint)RenderSettings.All;

        while (currentFlag < maxValue)
        {
            if ((currentFlag & (uint)settings) != 0)
            {
                builder.AppendLine(MacrosCache.renderSettingsCache[currentFlag]);
            }

            currentFlag <<= 1;
        }
    }

    private static void IncludeShadingModel(StringBuilder builder, ShadingModels model)
    {
        builder.AppendLine(MacrosCache.shadingModelCache[(byte)model]);
    }

    private static void IncludeSpecialSettings(StringBuilder builder, SpecialSettings settings)
    {
        uint currentFlag = 1;
        uint maxValue = (uint)SpecialSettings.UseMeshMatrix;

        while (currentFlag <= maxValue)
        {
            if ((currentFlag - (byte)settings) == 0)
                builder.AppendLine(MacrosCache.specialSettingsCache[currentFlag]);

            currentFlag <<= 1;
        }
    }

    private static void IncludeGlobalSetttings(StringBuilder builder, GlobalRenderSettings settings)
    {
        builder.AppendLine($"#define MAX_SHININESS {settings.MaxShininess}");
        builder.AppendLine($"#define AMBIENT_STRENGTH {settings.AmbientStrength}");

        if (settings.UseMonochromeAmbient)
            builder.AppendLine("#define USE_MONOCHROME_AMBIENT");
    }
}
