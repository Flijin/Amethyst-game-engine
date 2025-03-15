using Amethyst_game_engine.Render;

namespace Amethyst_game_engine.Core;

public struct Material
{
    private readonly Dictionary<uint, float> _materialValues = new()
    {
        //----Default values----//
        [1 << 2] = -1,
        [1 << 3] = -1,
        [1 << 4] = -1,
        [1 << 5] = -1,
        [1 << 6] = -1,
        [1 << 7] = -1,
        [1 << 8] = -1,
        [1 << 9] = -1,
        [1 << 11] = -1,
        [1 << 12] = -1,
        [1 << 13] = -1
    };

    private Color baseColorFactor = Color.NoneColor;

    internal uint materialKey;

    public Color BaseColorFactor
    {
        readonly get => baseColorFactor;

        set
        {
            baseColorFactor = value;

            if (value.isNoneColor)
                materialKey |= ~(uint)RenderSettings.BaseColorFactor;
            else
                materialKey |= (uint)RenderSettings.BaseColorFactor;
        }
    }

    public float MetallicFactor
    {
        readonly get => _materialValues[(uint)RenderSettings.MetallicFactor];

        set
        {
            var key = (uint)RenderSettings.MetallicFactor;

            if (value >= 0)
            {
                var clamptedValue = value <= 1f ? value : 1f;

                _materialValues[key] = clamptedValue;
                materialKey |= key;
            }
            else
            {
                _materialValues[key] = -1f;
                materialKey &= ~key;
            }
        }
    }

    public float RoughnessFactor
    {
        readonly get => _materialValues[(uint)RenderSettings.RoughnessFactor];

        set
        {
            var key = (uint)RenderSettings.RoughnessFactor;

            if (value >= 0)
            {
                var clamptedValue = value <= 1f ? value : 1f;

                _materialValues[key] = clamptedValue;
                materialKey |= key;
            }
            else
            {
                _materialValues[key] = -1f;
                materialKey &= ~key;
            }
        }
    }

    public float EmissiveFactor
    {
        readonly get => _materialValues[(uint)RenderSettings.EmissiveFactor];

        set
        {
            var key = (uint)RenderSettings.EmissiveFactor;

            if (value >= 0)
            {
                var clamptedValue = value <= 1f ? value : 1f;

                _materialValues[key] = clamptedValue;
                materialKey |= key;
            }
            else
            {
                _materialValues[key] = -1f;
                materialKey &= ~key;
            }
        }
    }

    public Material(){ }

    internal readonly float this[uint key]
    {
        get => _materialValues[key];
    }
}
