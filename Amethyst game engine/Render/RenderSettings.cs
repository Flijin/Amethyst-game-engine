namespace Amethyst_game_engine.Render;

[Flags]
public enum RenderSettings : uint
{
    [GLSLMacros("")]
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

    [GLSLMacros("#define USE_NORMAL_SCALE")]
    UseNormalScale = 1 << 10,

    [GLSLMacros("#define USE_OCCLUSION_STRENGTH")]
    UseOcclusionStrength = 1 << 11,

    [GLSLMacros(true)]
    All = (1 << 12) - 1
}
