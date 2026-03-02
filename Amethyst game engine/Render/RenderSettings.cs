namespace Amethyst_game_engine.Render;

[Flags]
public enum RenderSettings : uint
{
    [GLSLMacros("")]
    None = 0,

    [GLSLMacros("#define USE_VERTEX_COLORS")]
    VertexColors = 1 << 0,

    [GLSLMacros("#define USE_LIGHTING")]
    Lighting = 1 << 1,

    [UniformName("_albedoTexture")]
    [GLSLMacros("#define USE_ALBEDO_MAP")]
    AlbedoMap = 1 << 2,

    [UniformName("_metallicRoughnessTexture")]
    [GLSLMacros("#define USE_METALLIC_ROUGHNESS_MAP")]
    MetallicRoughnessMap = 1 << 3,

    [UniformName("_normalTexture")]
    [GLSLMacros("#define USE_NORMAL_MAP")]
    NormalMap = 1 << 4,

    [UniformName("_occlusionTexture")]
    [GLSLMacros("#define USE_OCCLUSION_MAP")]
    OcclusionMap = 1 << 5,

    [UniformName("_emissiveTexture")]
    [GLSLMacros("#define USE_EMISSIVE_MAP")]
    EmissiveMap = 1 << 6,

    [UniformName("_baseColorFactor")]
    [GLSLMacros("#define USE_BASE_COLOR_FACTOR")]
    BaseColorFactor = 1 << 7,

    [UniformName("_metallicFactor")]
    [GLSLMacros("#define USE_METALLIC_FACTOR")]
    MetallicFactor = 1 << 8,

    [UniformName("_roughnessFactor")]
    [GLSLMacros("#define USE_ROUGHNESS_FACTOR")]
    RoughnessFactor = 1 << 9,

    [UniformName("_emissiveFactor")]
    [GLSLMacros("#define USE_EMISSIVE_FACTOR")]
    EmissiveFactor = 1 << 10,

    [UniformName("_normalScale")]
    [GLSLMacros("#define USE_NORMAL_SCALE")]
    UseNormalScale = 1 << 11,

    [UniformName("_occlusionStrength")]
    [GLSLMacros("#define USE_OCCLUSION_STRENGTH")]
    UseOcclusionStrength = 1 << 12,

    [GLSLMacros(true)]
    All = (1 << 13) - 1
}
