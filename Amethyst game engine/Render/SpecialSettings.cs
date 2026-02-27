namespace Amethyst_game_engine.Render;

[Flags]
internal enum SpecialSettings : byte
{
    [GLSLMacros("")]
    None = 0,

    [GLSLMacros("#define USE_NORMAL_SCALE")]
    UseNormalScale = 1 << 0,

    [GLSLMacros("#define USE_OCCLUSION_STRENGTH")]
    UseOcclusionStrength = 1 << 1,

    [GLSLMacros("#define USE_MESH_MATRIX")]
    UseMeshMatrix = 1 << 2,
}