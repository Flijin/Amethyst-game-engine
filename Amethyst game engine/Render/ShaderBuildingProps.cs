using System.Diagnostics.CodeAnalysis;

namespace Amethyst_game_engine.Render;

internal sealed class ShaderBuildingProps
{
    public RenderSettings RenderSettings { get; set; }
    public ShadingModels ShadingModel { get; set; }
    public SpecialSettings SpecialSettings { get; set; }
    public GlobalRenderSettings GlobalSettings { get; set; }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is ShaderBuildingProps props &&
            props.RenderSettings == RenderSettings &&
            props.ShadingModel == ShadingModel &&
            props.SpecialSettings == SpecialSettings &&
            props.GlobalSettings == GlobalSettings;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(RenderSettings, ShadingModel,
                                SpecialSettings, GlobalSettings);
    }
}
