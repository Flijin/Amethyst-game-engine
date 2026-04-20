using Amethyst_game_engine.Core.Render.Components;

namespace Amethyst_game_engine.Core.Render.Settings;

[Flags]
public enum RenderSettings : uint
{
    None = 0,

    [GLSLMacros("#define USE_VERTEX_COLORS")]
    VertexColors = 1 << 0,

    [GLSLMacros("#define USE_ALBEDO_MAP")]
    AlbedoMap = 1 << 1,

    [GLSLMacros("#define USE_METALLIC_ROUGHNESS_MAP")]
    MetallicRoughnessMap = 1 << 2,

    [GLSLMacros("#define USE_NORMAL_MAP")]
    NormalMap = 1 << 3,

    [GLSLMacros("#define USE_OCCLUSION_MAP")]
    OcclusionMap = 1 << 4,

    [GLSLMacros("#define USE_EMISSIVE_MAP")]
    EmissiveMap = 1 << 5,

    [GLSLMacros("#define USE_BASE_COLOR_FACTOR")]
    BaseColorFactor = 1 << 6,

    [GLSLMacros("#define USE_METALLIC_FACTOR")]
    MetallicFactor = 1 << 7,

    [GLSLMacros("#define USE_ROUGHNESS_FACTOR")]
    RoughnessFactor = 1 << 8,

    [GLSLMacros("#define USE_EMISSIVE_FACTOR")]
    EmissiveFactor = 1 << 9,

    All = (1 << 10) - 1
}
