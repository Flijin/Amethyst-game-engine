using Amethyst_game_engine.Render;

namespace Amethyst_game_engine.Core;

public class Material
{
    private RenderSettings _materialKey = RenderSettings.None;
    private Color _baseColorFactor;

    internal int albedoMap0;
    internal int albedoMap1;
    internal int albedoMap2;
    internal int albedoMap3;

    internal int metallicRoughnessMap;
    internal int normalMap;
    internal int occlusionMap;
    internal int emissiveMap;
    internal float occlusionStrength;
    internal float normalScale;

    private float _metallicFactor;
    private float _roughnessFactor;
    private float _emissiveFactor;

    internal RenderSettings GetMaterialKey => _materialKey;

    public Color BaseColorFactor
    {
        get => _baseColorFactor;

        set
        {
            _baseColorFactor = value;
        }
    }

    public float MetallicFactor
    {
        get => _metallicFactor;

        set
        {
            if (value >= 0)
            {
                _metallicFactor = value <= 1f ? value : 1f;
                _materialKey |= RenderSettings.MetallicFactor;
            }
            else
            {
                _metallicFactor = -1f;
                _materialKey &= ~RenderSettings.MetallicFactor;
            }
        }
    }

    public float RoughnessFactor
    {
        get => _roughnessFactor;

        set
        {
            if (value >= 0)
            {
                _roughnessFactor = value <= 1f ? value : 1f;
                _materialKey |= RenderSettings.RoughnessFactor;
            }
            else
            {
                _metallicFactor = -1f;
                _materialKey &= ~RenderSettings.RoughnessFactor;
            }
        }
    }

    public float EmissiveFactor
    {
        get => _emissiveFactor;

        set
        {
            if (value >= 0)
            {
                _emissiveFactor = value <= 1f ? value : 1f;
                _materialKey |= RenderSettings.EmissiveFactor;
            }
            else
            {
                _emissiveFactor = -1f;
                _materialKey &= ~RenderSettings.EmissiveFactor;
            }
        }
    }

    internal Material(RenderSettings keys) => _materialKey = keys;

    public Material(Color baseColorFactor)
    {
        BaseColorFactor = baseColorFactor;
        _materialKey |= RenderSettings.BaseColorFactor;
    }
}
