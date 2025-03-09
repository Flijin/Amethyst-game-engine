using Amethyst_game_engine.Models;
using System.Runtime.CompilerServices;
using System.Text;

namespace Amethyst_game_engine.Render;

public static class GLSLMacrosExtensions
{
    private static readonly Dictionary<int, string> _tokens = new()
    {
        //------RenderSettings------//
        [1] = "USE_VERTEX_COLORS",
        [1 << 1] = "USE_NORMALS",
        [1 << 2] = "USE_ALBEDO_MAP_0",
        [1 << 3] = "USE_ALBEDO_MAP_1",
        [1 << 4] = "USE_ALBEDO_MAP_2",
        [1 << 5] = "USE_ALBEDO_MAP_3",
        [1 << 6] = "USE_METALLIC_ROUGHNESS_MAP",
        [1 << 7] = "USE_NORMAL_MAP",
        [1 << 8] = "USE_OCCLUSION_MAP",
        [1 << 9] = "USE_EMISSIVE_MAP",
        [1 << 10] = "USE_BASE_COLOR_FACTOR",
        [1 << 11] = "USE_METALLIC_FACTOR",
        [1 << 12] = "USE_ROUGHNESS_FACTOR",
        [1 << 13] = "USE_EMISSIVE_FACTOR",
        [1 << 14] = "USE_OCCLUSION_STRENGTH",
        [1 << 15] = "USE_NORMAL_SCALE",

        //------ModelSettings------//
        [1 << 24] = "USE_MESH_MATRIX",
        [1 << 25] = "USE_COLOR_5_BITS",
    };

    public static string ToMacrosString(this RenderSettings exObj) => ToMacrosString((int)exObj, 1);
    internal static string ToMacrosString(this ModelSettings exObj) => ToMacrosString((int)exObj, 1 << 24);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ToMacrosString(int tokens, int startDigit)
    {
        if (tokens == 0)
            return string.Empty;

        var builder = new StringBuilder("#define ", 16);

        while (startDigit <= tokens)
        {
            if ((tokens & startDigit) > 0)
                builder.Append(_tokens[startDigit] + ", ");

            startDigit <<= 1;
        }

        return builder.ToString().Trim(',', ' ');
    }
}
