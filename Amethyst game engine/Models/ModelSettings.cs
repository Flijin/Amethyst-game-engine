namespace Amethyst_game_engine.Models;

[Flags]
internal enum ModelSettings : uint
{
    USE_MESH_MATRIX = 1 << 24,
    USE_COLOR_5_BITS = 1 << 25
}
