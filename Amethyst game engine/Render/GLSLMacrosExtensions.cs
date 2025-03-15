﻿#define DEBUG_MODE

using System.Runtime.CompilerServices;
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

        //------Special Flag------//
        [1 << 24] = "USE_MESH_MATRIX",
    };


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

#if DEBUG_MODE
        System.Diagnostics.Debug.WriteLine(builder + " {0} times called", ++_timesCalled);
#endif

        return builder.ToString();
    }
}
