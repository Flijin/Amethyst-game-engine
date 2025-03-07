namespace Amethyst_game_engine.Render;

[Flags]
public enum RenderSettings
{
    None = 0,
    VertexColors = 1,
    Normals = 1 << 1,

    AlbedoMap_0 = 1 << 2,
    AlbedoMap_1 = 1 << 3,
    AlbedoMap_2 = 1 << 4,
    AlbedoMap_3 = 1 << 5,

    MetallicRoughnessMap = 1 << 6,
    NormalMap = 1 << 7,
    OcclusionMap = 1 << 8,
    EmissiveMap = 1 << 9,

    BaseColorFactor = 1 << 10,
    MetallicFactor = 1 << 11,
    RoughnessFactor = 1 << 12,
    EmissiveFactor = 1 << 13,

    OcclusionStrength = 1 << 14,
    NormalScale = 1 << 15,

    All = 65535
}

// Keys
//----------------------
// STL 0011110000000011
// GLB 1111111111111111
