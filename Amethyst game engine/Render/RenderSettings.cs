namespace Amethyst_game_engine.Render;

[Flags]
public enum RenderSettings : uint
{
    None = 0,
    VertexColors = 1,
    Lighting = 1 << 1,

    AlbedoMap = 1 << 2,
    MetallicRoughnessMap = 1 << 3,
    NormalMap = 1 << 4,
    OcclusionMap = 1 << 5,
    EmissiveMap = 1 << 6,

    BaseColorFactor = 1 << 7,
    MetallicFactor = 1 << 8,
    RoughnessFactor = 1 << 9,
    EmissiveFactor = 1 << 10,

    OcclusionStrength = 1 << 11,
    NormalScale = 1 << 12,

    All = 0b_00011111_11111111
}
