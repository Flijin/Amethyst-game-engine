using Amethyst_game_engine.Core.Render.Components;

namespace Amethyst_game_engine.Core.Render.Settings;

[Flags]
public enum ShadingModels : byte
{
    [GLSLMacros("#define USE_BLINN_PHONG")]
    BlinnPhong = 0,

    [GLSLMacros("#define USE_GOURAUD")]
    Gouraud = 1,

    [GLSLMacros("#define USE_PBR_METALLIC_ROUGHNESS")]
    PBR_MetallicRoughness = 2,

    [GLSLMacros("#define USE_UNLIT")]
    Unlit = 3
}
