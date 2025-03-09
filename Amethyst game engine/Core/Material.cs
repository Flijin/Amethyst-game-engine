using Amethyst_game_engine.Models;
using Amethyst_game_engine.Render;
using System.Reflection;

namespace Amethyst_game_engine.Core;

public class Material
{
    private Color _baseColorFactor;
    internal uint materialKey;

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

    internal uint MaterialKey => materialKey;

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
                materialKey |= (uint)RenderSettings.MetallicFactor;
            }
            else
            {
                _metallicFactor = -1f;
                materialKey &= ~(uint)RenderSettings.MetallicFactor;
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
                materialKey |= (uint)RenderSettings.RoughnessFactor;
            }
            else
            {
                _metallicFactor = -1f;
                materialKey &= ~(uint)RenderSettings.RoughnessFactor;
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
                materialKey |= (uint)RenderSettings.EmissiveFactor;
            }
            else
            {
                _emissiveFactor = -1f;
                materialKey &= ~(uint)RenderSettings.EmissiveFactor;
            }
        }
    }

    internal Material(uint Materialkeys) => materialKey = Materialkeys;

    public Material(Color baseColorFactor)
    {
        BaseColorFactor = baseColorFactor;
        materialKey |= (int)RenderSettings.BaseColorFactor | (int)ModelSettings.USE_COLOR_5_BITS;
    }
}
