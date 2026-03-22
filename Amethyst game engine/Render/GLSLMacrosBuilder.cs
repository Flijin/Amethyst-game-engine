using System.Globalization;
using System.Text;

namespace Amethyst_game_engine.Render;

internal static class GLSLMacrosBuilder
{
    public static void BuildMacrosByFlags(ShaderBuildingProps props, StringBuilder builder)
    {
        IncludeRenderSetingsMacros(builder, props.RenderSettings);
        IncludeShadingModel(builder, props.ShadingModel);
        IncludeGlobalSetttings(builder, props.GlobalSettings);

        if (props.UseMeshMatrix)
            builder.AppendLine("#define USE_MESH_MATRIX");
    }

    private static void IncludeRenderSetingsMacros(StringBuilder builder, RenderSettings settings)
    {
        uint currentFlag = 1;
        uint maxValue = (uint)RenderSettings.All;

        while (currentFlag < maxValue)
        {
            if ((currentFlag & (uint)settings) != 0)
            {
                builder.AppendLine(MacrosCache.RenderSettingsCache[currentFlag]);
            }

            currentFlag <<= 1;
        }
    }

    private static void IncludeShadingModel(StringBuilder builder, ShadingModels model)
    {
        builder.AppendLine(MacrosCache.ShadingModelCache[(byte)model]);
    }

    private static void IncludeGlobalSetttings(StringBuilder builder, GlobalRenderSettings settings)
    {
        builder.AppendLine($"#define MAX_SHININESS {settings.MaxShininess}");
        builder.AppendLine($"#define AMBIENT_STRENGTH {settings.AmbientStrength.ToString(CultureInfo.InvariantCulture)}");

        if (settings.UseMonochromeAmbient)
            builder.AppendLine("#define USE_MONOCHROME_AMBIENT");
    }
}
