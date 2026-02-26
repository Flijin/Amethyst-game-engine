using Amethyst_game_engine.Render;
using OpenTK.Graphics.OpenGL4;

namespace Amethyst_game_engine.Core;

public struct Material
{
    internal enum TexturesType : byte
    {
        AlbedoMap = 0,
        MetallicRoughnessMap = 1,
        NormalMap = 2,
        OcclusionMap = 3,
        EmissiveMap = 4,
    }

    internal enum FactorsType : byte
    {
        BaseColorFactor = 0,
        MetallicFactor = 1,
        RoughnessFactor = 2,
        EmissiveFactor = 3,
    }

    internal RenderSettings materialKey;

    internal readonly (RenderSettings settings, Texture? texture)[] textures = new (RenderSettings, Texture?)[5];
    internal readonly (RenderSettings settings, object factor)[] factors = new (RenderSettings, object)[4];

    internal Texture? this[TexturesType type]
    {
        readonly get => textures[(int)type].texture;

        set
        {
            textures[(int)type] = (GetSettingsByTextureType(type), value);
            UpdateTexturesFlags(type, value != null);
        }
    }

    internal object this[FactorsType factor]
    {
        readonly get => factors[(int)factor].factor;

        set
        {
            factors[(int)factor] = (GetSettingsByFactorType(factor), value);
            UpdateFactorsFlags(factor, value is float val ? val >= 0 : ((Color)value).isNoneColor == false);
        }
    }

    internal Texture? AlbedoTexture
    {
        readonly get => this[TexturesType.AlbedoMap];

        set => this[TexturesType.AlbedoMap] = value;
    }

    internal Texture? MetallicRoughnessMap
    {
        readonly get => this[TexturesType.MetallicRoughnessMap];

        set => this[TexturesType.MetallicRoughnessMap] = value;
    }

    internal Texture? NormalMap
    {
        readonly get => this[TexturesType.NormalMap];

        set => this[TexturesType.NormalMap] = value;
    }

    internal Texture? OcclusionMap
    {
        readonly get => this[TexturesType.OcclusionMap];

        set => this[TexturesType.OcclusionMap] = value;
    }

    internal Texture? EmissiveMap
    {
        readonly get => this[TexturesType.EmissiveMap];

        set => this[TexturesType.EmissiveMap] = value;
    }

    public Color BaseColorFactor
    {
        readonly get => (Color)this[FactorsType.BaseColorFactor];

        set => this[FactorsType.BaseColorFactor] = value;
    }

    public float MetallicFactor
    {
        readonly get => (float)this[FactorsType.MetallicFactor];

        set => this[FactorsType.MetallicFactor] = value;
    }

    public float RoughnessFactor
    {
        readonly get => (float)this[FactorsType.RoughnessFactor];

        set => this[FactorsType.RoughnessFactor] = value;
    }

    public Color EmissiveFactor
    {
        readonly get => (Color)this[FactorsType.EmissiveFactor];

        set => this[FactorsType.EmissiveFactor] = value;
    }

    private void UpdateTexturesFlags(TexturesType type, bool condition)
    {
        var renderSetting = GetSettingsByTextureType(type);

        if (condition)
            materialKey |= renderSetting;
        else
            materialKey &= ~renderSetting;
    }

    private void UpdateFactorsFlags(FactorsType type, bool condition)
    {
        var renderSetting = GetSettingsByFactorType(type);

        if (condition)
            materialKey |= renderSetting;
        else
            materialKey &= ~renderSetting;
    }
    
    private static RenderSettings GetSettingsByTextureType(TexturesType type)
    {
        return type switch
        {
            TexturesType.AlbedoMap => RenderSettings.AlbedoMap,
            TexturesType.MetallicRoughnessMap => RenderSettings.MetallicRoughnessMap,
            TexturesType.NormalMap => RenderSettings.NormalMap,
            TexturesType.OcclusionMap => RenderSettings.OcclusionMap,
            TexturesType.EmissiveMap => RenderSettings.EmissiveMap,

            _ => RenderSettings.None
        };
    }

    private static RenderSettings GetSettingsByFactorType(FactorsType type)
    {
        return type switch
        {
            FactorsType.BaseColorFactor => RenderSettings.BaseColorFactor,
            FactorsType.MetallicFactor => RenderSettings.MetallicFactor,
            FactorsType.RoughnessFactor => RenderSettings.RoughnessFactor,
            FactorsType.EmissiveFactor => RenderSettings.EmissiveFactor,

            _ => RenderSettings.None
        };
    }

    public Material()
    {
        materialKey = RenderSettings.None;
    }
}
