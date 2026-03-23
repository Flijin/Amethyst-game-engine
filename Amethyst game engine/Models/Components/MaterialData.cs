using Amethyst_game_engine.Core.Utilities;

namespace Amethyst_game_engine.Models.Components;

internal sealed class MaterialData
{
    public byte[]? AlbedoMap { get; set; }
    public byte[]? MetallicRoughnessMap { get; set; }
    public byte[]? NormalMap { get; set; }
    public byte[]? OcclusionMap { get; set; }
    public byte[]? EmissiveMap { get; set; }

    public Color BaseColorFactor { get; set; } = Color.NoneColor;
    public float MetallicFactor { get; set; } = -1;
    public float RoughnessFactor { get; set; } = -1;
    public Color EmissiveFactor { get; set; } = Color.NoneColor;
}
