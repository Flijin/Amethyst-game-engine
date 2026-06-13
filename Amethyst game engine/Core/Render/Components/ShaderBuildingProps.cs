using System.Diagnostics.CodeAnalysis;
using Amethyst_game_engine.Core.Render.Settings;

namespace Amethyst_game_engine.Core.Render.Components;

public sealed class ShaderBuildingProps
{
    public RenderSettings RenderSettings { get; set; }
    public ShadingModels ShadingModel { get; set; }
    public GlobalRenderSettings GlobalSettings { get; set; }
    public bool UseMeshMatrix { get; set; }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is ShaderBuildingProps props &&
            props.RenderSettings == RenderSettings &&
            props.ShadingModel == ShadingModel &&
            props.GlobalSettings == GlobalSettings;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(RenderSettings, ShadingModel, GlobalSettings);
    }
}
