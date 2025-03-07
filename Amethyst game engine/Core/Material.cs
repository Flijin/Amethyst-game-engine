namespace Amethyst_game_engine.Core;

public class Material
{
    internal int albedoMap0 = -1;
    internal int albedoMap1 = -1;
    internal int albedoMap2 = -1;
    internal int albedoMap3 = -1;

    internal int metallicRoughnessMap = -1;
    internal int normalMap = -1;
    internal int occlusionMap = -1;
    internal int emissiveMap = -1;
    internal float occlusionStrength = -1;

    private float _metallicFactor = -1;
    private float _roughnessFactor = -1;
    private float _emissiveFactor = -1;
    private float _normalScale = -1;

    public static Material NoneMaterial => new();
    public Color BaseColorFactor { get; set; } = new Color();

    internal int GetMaterialKey
    {
        get
        {
            int result =
                (albedoMap0 >>> 31) * 4 +
                (albedoMap1 >>> 31) * 8 +
                (albedoMap2 >>> 31) * 16 +
                (albedoMap3 >>> 31) * 32 +
                (metallicRoughnessMap >>> 31) * 64 +
                (normalMap >>> 31) * 128 +
                (occlusionMap >>> 31) * 256 +
                (emissiveMap >>> 31) * 512 +
                ((int)_metallicFactor >>> 31) * 2048 +
                ((int)_roughnessFactor >>> 31) * 4096 +
                ((int)_emissiveFactor >>> 31) * 8192 +
                ((int)occlusionStrength >>> 31) * 16384 +
                ((int)_normalScale >>> 31) * 32768;

            if (BaseColorFactor.isNoneColor)
                result += 1024;

            return result;
        }
    }

    public float MetallicFactor
    {
        get => _metallicFactor;

        set
        {
            _metallicFactor = Mathematics.Clamp(value, 0f, 1f);
        }
    }

    public float RoughnessFactor
    {
        get => _roughnessFactor;

        set
        {
            _roughnessFactor = Mathematics.Clamp(value, 0f, 1f);
        }
    }

    public float EmissiveFactor
    {
        get => _emissiveFactor;

        set
        {
            _emissiveFactor = Mathematics.Clamp(value, 0f, 1f);
        }
    }

    public float NormalScale
    {
        get => _normalScale;

        set
        {
            _normalScale = Mathematics.Clamp(value, 0f, float.MaxValue);
        }
    }
}
