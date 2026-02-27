namespace Amethyst_game_engine.Render;

internal class ShaderBuildingProps
{
    public RenderSettings RenderSettings { get; set; }
    public ShadingModels ShadingModel { get; set; }
    public SpecialSettings SpecialSettings { get; set; }
    public required GlobalRenderSettings GlobalSettings { get; set; }
}
