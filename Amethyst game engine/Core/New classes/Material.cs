using Amethyst_game_engine.Render;

namespace Amethyst_game_engine.Core.New_classes;

public struct Material
{
    internal readonly Texture?[] textures = new Texture[5];

    private RenderSettings _materialKey;

    private Color _baseColorFactor;
    private Color _emissiveFactor;
    private float _metallicFactor;
    private float _roughnessFactor;

    public readonly RenderSettings MaterialKey => _materialKey;

    internal Texture? AlbedoMap
    {
        readonly get => textures[0];

        set
        {
            textures[0] = value;
            UpdateFlag(RenderSettings.AlbedoMap, value != null);
        }
    }

    internal Texture? MetallicRoughnessMap
    {
        readonly get => textures[1];

        set
        {
            textures[1] = value;
            UpdateFlag(RenderSettings.MetallicRoughnessMap, value != null);
        }
    }

    internal Texture? NormalMap
    {
        readonly get => textures[2];

        set
        {
            textures[2] = value;
            UpdateFlag(RenderSettings.NormalMap, value != null);
        }
    }

    internal Texture? OcclusionMap
    {
        readonly get => textures[3];

        set
        {
            textures[3] = value;
            UpdateFlag(RenderSettings.OcclusionMap, value != null);
        }
    }

    internal Texture? EmissiveMap
    {
        readonly get => textures[4];

        set
        {
            textures[4] = value;
            UpdateFlag(RenderSettings.EmissiveMap, value != null);
        }
    }

    public Color BaseColorFactor
    {
        readonly get => _baseColorFactor;

        set
        {
            _baseColorFactor = value;
            UpdateFlag(RenderSettings.BaseColorFactor, value.IsNoneColor == false);
        }
    }

    public float MetallicFactor
    {
        readonly get => _metallicFactor;

        set
        {
            _metallicFactor = value;
            UpdateFlag(RenderSettings.MetallicFactor, value >= 0 && value <= 1);
        }
    }

    public float RoughnessFactor
    {
        readonly get => _roughnessFactor;

        set
        {
            _roughnessFactor = value;
            UpdateFlag(RenderSettings.RoughnessFactor, value >= 0 && value <= 1);
        }
    }

    public Color EmissiveFactor
    {
        readonly get => _emissiveFactor;

        set
        {
            _emissiveFactor = value;
            UpdateFlag(RenderSettings.EmissiveFactor, value.IsNoneColor == false);
        }
    }

    private void UpdateFlag(RenderSettings setting, bool condition)
    {
        if (condition)
            _materialKey |= setting;
        else
            _materialKey &= ~setting;
    }

    public Material() => _materialKey = RenderSettings.None;
}
