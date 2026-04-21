using Amethyst_game_engine.Core.Render.Settings;
using Amethyst_game_engine.Core.Utilities;

namespace Amethyst_game_engine.Models.Components;

internal sealed class MaterialData
{
    public RenderSettings Flags { get; set; }

    public TextureData? AlbedoMap { get; set; }
    public TextureData? MetallicRoughnessMap { get; set; }
    public (TextureData, float)? NormalMap { get; set; }
    public (TextureData, float)? OcclusionMap { get; set; }
    public TextureData? EmissiveMap { get; set; }

    public Color BaseColorFactor { get; set; } = Color.NoneColor;
    public float MetallicFactor { get; set; } = -1;
    public float RoughnessFactor { get; set; } = -1;
    public Color EmissiveFactor { get; set; } = Color.NoneColor;
}
