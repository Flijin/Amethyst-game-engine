namespace Amethyst_game_engine.Models;

[Flags]
internal enum MaterialsProperties : byte
{
    Albedo = 0,
    MetallicRoughness = 1,
    Normal = 2,
    Occlusion = 4,
    Emissive = 8
}
