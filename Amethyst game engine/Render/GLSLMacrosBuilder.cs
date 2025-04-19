//#define DEBUG_MODE

using Amethyst_game_engine.Core.Light;
using OpenTK.Graphics.ES11;
using System.Globalization;
using System.Text;

namespace Amethyst_game_engine.Render;

internal static class GLSLMacrosBuilder
{
#if DEBUG_MODE
    private static int _timesCalled;
#endif

    public static readonly Dictionary<uint, string> _tokens = new()
    {
        //------RenderSettings------//
        [1] = "USE_VERTEX_COLORS",
        [1 << 1] = "USE_LIGHTING",
        [1 << 2] = "USE_ALBEDO_MAP",
        [1 << 3] = "USE_METALLIC_ROUGHNESS_MAP",
        [1 << 4] = "USE_NORMAL_MAP",
        [1 << 5] = "USE_OCCLUSION_MAP",
        [1 << 6] = "USE_EMISSIVE_MAP",
        [1 << 7] = "USE_BASE_COLOR_FACTOR",
        [1 << 8] = "USE_METALLIC_FACTOR",
        [1 << 9] = "USE_ROUGHNESS_FACTOR",
        [1 << 10] = "USE_EMISSIVE_FACTOR",
        [1 << 11] = "USE_OCCLUSION_STRENGTH",
        [1 << 12] = "USE_NORMAL_SCALE",

        //------Special Flag------//
        [1 << 24] = "USE_MESH_MATRIX",
    };

    public static string GetBuildData(uint renderTokens, uint shadingModel, bool useLighting)
    {
        StringBuilder builder = new();

        RenderKeysToMacros(renderTokens, builder);
        GlobalRenderSettingsToMacros(builder);
        ShadingModelToMacros(shadingModel, useLighting, builder);
        UBOSettingsToMacros(builder);

        return builder.ToString();
    }

    private static void RenderKeysToMacros(uint tokens, StringBuilder builder)
    {
        if (tokens == 0)
            return;

        uint startDigit = 1;

        while (startDigit <= tokens)
        {
            if ((tokens & startDigit) > 0)
                builder.Append("#define " + _tokens[startDigit] + "\r\n");

            startDigit <<= 1;
        }

#if DEBUG_MODE
        System.Diagnostics.Debug.WriteLine(builder + " {0} times called", ++_timesCalled);
#endif
    }

    private static void GlobalRenderSettingsToMacros(StringBuilder builder)
    {
        builder.AppendLine($"#define MAX_SHININESS {GlobalRenderSettings.MaxShininess}");
        builder.AppendLine($"#define AMBIENT_STHENGTH {GlobalRenderSettings.AmbientStrength.ToString(CultureInfo.InvariantCulture)}");
        builder.AppendLine($"#define USE_MONOCHROME_AMBIENT {GlobalRenderSettings.UseMonochromeAmbient}");
    }

    private static void ShadingModelToMacros(this uint shadingModel, bool useLighting, StringBuilder builder)
    {
        if (useLighting)
        {
            builder.AppendLine(shadingModel switch
            {
                0 => "#define USE_BLINN_PHONG_SHADING_MODEL",
                1 => "#define USE_GOURAND_SHADING_MODEL",
                2 => "#define USE_lAMBERTIAN_SHADING_MODEL",
                3 => "#define USE_OREN_NAYAR_SHADING_MODEL",
                4 => "#define USE_DISNEY_BRDF_SHADING_MODEL",

                _ => string.Empty
            });
        }
    }

    private static void UBOSettingsToMacros(StringBuilder builder)
    {
        builder.AppendLine($"#define DIRECTIONAL_LIGHTS_COUNT {LightManager.MaxDirectionLights}");
        builder.AppendLine($"#define POINT_LIGHTS_COUNT {LightManager.MaxPointLights}");
        builder.AppendLine($"#define SPOTLIGHT_COUNT {LightManager.MaxSpotlights}");
    }
}
