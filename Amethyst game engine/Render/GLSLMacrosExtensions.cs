//#define DEBUG_MODE

using System.Globalization;
using System.Text;

namespace Amethyst_game_engine.Render;

internal static class GLSLMacrosExtensions
{
#if DEBUG_MODE
    private static int _timesCalled;
#endif

    private static readonly Dictionary<uint, string> _tokens = new()
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

    internal static string ToMacrosString(this uint tokens)
    {
        if (tokens == 0)
            return string.Empty;

        uint startDigit = 1;
        var builder = new StringBuilder();

        while (startDigit <= tokens)
        {
            if ((tokens & startDigit) > 0)
                builder.Append("#define " + _tokens[startDigit] + "\r\n");

            startDigit <<= 1;
        }

        builder.Append($"#define MAX_SHININESS {GlobalRenderSettings.MaxShininess}" + "\r\n");
        builder.Append($"#define AMBIENT_STHENGTH {GlobalRenderSettings.AmbientStrength.ToString(CultureInfo.InvariantCulture)}" + "\r\n");
        builder.Append($"#define USE_MONOCHROME_AMBIENT {GlobalRenderSettings.UseMonochromeAmbient}" + "\r\n");


#if DEBUG_MODE
        System.Diagnostics.Debug.WriteLine(builder + " {0} times called", ++_timesCalled);
#endif

        return builder.ToString();
    }
}
