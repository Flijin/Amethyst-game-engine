namespace Amethyst_game_engine.Core;

public class Material
{
    internal readonly int albedoMap = -1;
    internal readonly int MetallicRoughnessMap = -1;
    internal readonly int NormalMap = -1;
    internal readonly int OcclusionMap = -1;
    internal readonly int EmissiveMap = -1;

    internal float metallicFactor = 1.0f;
    internal float roughnessFactor = 1.0f;
    internal float baseColorFactor = 1.0f;
    internal float emissiveFactor = 1.0f;

    internal readonly float occlusionStrength = 1.0f;
    internal readonly float normalScale = 1.0f;

    public float MetallicFactor
    {
        get => metallicFactor;

        init => metallicFactor = Mathematics.Clamp(value, 0f, 1f);
    }

    public float RoughnessFactor
    {
        get => roughnessFactor;

        init => roughnessFactor = Mathematics.Clamp(value, 0f, 1f);
    }

    public float BaseColorFactor
    {
        get => baseColorFactor;

        init => baseColorFactor = Mathematics.Clamp(value, 0f, 1f);
    }

    public float EmissiveFactor
    {
        get => emissiveFactor;

        init => emissiveFactor = Mathematics.Clamp(value, 0f, 1f);
    }
}
