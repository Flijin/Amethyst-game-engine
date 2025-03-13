using Amethyst_game_engine.Models;
using Amethyst_game_engine.Render;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Amethyst_game_engine.Core;

/*
 * _albedoMap0
 * _albedoMap1
 * _albedoMap2
 * _albedoMap3
 * _metallicRoughnessMap
 * _normalMap
 * _occlusionMap
 * _emissiveMap
 * _occlusionStrength
 * _normalScale
 * _metallicFactor
 * _roughnessFactor
 * _emissiveFactor
 * _baseColorFactor
 */

public class Material
{
    internal readonly Dictionary<string, int> uniforms_int = [];
    internal readonly Dictionary<string, float> uniforms_float = [];
    internal (string? uniform, Color value) baseColorFactor;

    internal uint materialKey;

    public Color BaseColorFactor
    {
        get => baseColorFactor.value;

        set
        {
            baseColorFactor.value = value;
        }
    }

    public float MetallicFactor
    {
        get
        {
            if (uniforms_float.TryGetValue("_metallicFactor", out float res))
                return res;

            return -1f;
        }

        set
        {
            if (value >= 0)
            {
                var clamptedValue = value <= 1f ? value : 1f;

                if (uniforms_float.TryAdd("_metallicFactor", clamptedValue) == false)
                    uniforms_float["_metallicFactor"] = clamptedValue;

                materialKey |= (uint)RenderSettings.MetallicFactor;
            }
            else
            {
                uniforms_float.Remove("_metallicFactor");
                materialKey &= ~(uint)RenderSettings.MetallicFactor;
            }
        }
    }

    public float RoughnessFactor
    {
        get
        {
            if (uniforms_float.TryGetValue("_roughnessFactor", out float res))
                return res;

            return -1f;
        }

        set
        {
            if (value >= 0)
            {
                var clamptedValue = value <= 1f ? value : 1f;

                if (uniforms_float.TryAdd("_roughnessFactor", clamptedValue) == false)
                    uniforms_float["_roughnessFactor"] = clamptedValue;

                materialKey |= (uint)RenderSettings.RoughnessFactor;
            }
            else
            {
                uniforms_float.Remove("_roughnessFactor");
                materialKey &= ~(uint)RenderSettings.RoughnessFactor;
            }
        }
    }

    public float EmissiveFactor
    {
        get
        {
            if (uniforms_float.TryGetValue("_emissiveFactor", out float res))
                return res;

            return -1f;
        }

        set
        {
            if (value >= 0)
            {
                var clamptedValue = value <= 1f ? value : 1f;

                if (uniforms_float.TryAdd("_emissiveFactor", clamptedValue) == false)
                    uniforms_float["_emissiveFactor"] = clamptedValue;

                materialKey |= (uint)RenderSettings.EmissiveFactor;
            }
            else
            {
                uniforms_float.Remove("_emissiveFactor");
                materialKey &= ~(uint)RenderSettings.EmissiveFactor;
            }
        }
    }

    internal Material(uint Materialkeys) => materialKey = Materialkeys;

    public Material(Color baseColorFactor)
    {
        BaseColorFactor = baseColorFactor;
        materialKey |= (uint)RenderSettings.BaseColorFactor;

        this.baseColorFactor.uniform = "_baseColorFactor";
    }
}
