using System.Diagnostics.CodeAnalysis;

namespace Amethyst_game_engine.Render;

public struct GlobalRenderSettings()
{
    public static GlobalRenderSettings Default => new()
    {
        MaxShininess = 32,
        AmbientStrength = 0.1f,
        UseMonochromeAmbient = true,
    };

    public float MaxShininess { get; set; }

    public float AmbientStrength { get; set; }

    public bool UseMonochromeAmbient { get; set; }

    public static bool operator ==(GlobalRenderSettings param1, GlobalRenderSettings param2) => param1.Equals(param2);

    public static bool operator !=(GlobalRenderSettings param1, GlobalRenderSettings param2) => param1.Equals(param2) == false;

    public readonly override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is GlobalRenderSettings settings &&
            settings.MaxShininess == MaxShininess &&
            settings.AmbientStrength == AmbientStrength &&
            settings.UseMonochromeAmbient == UseMonochromeAmbient;
    }

    public readonly override int GetHashCode()
    {
        return HashCode.Combine(MaxShininess, AmbientStrength, UseMonochromeAmbient);
    }
}
