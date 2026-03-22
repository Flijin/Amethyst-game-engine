using Amethyst_game_engine.Render;

namespace Amethyst_game_engine.Core.New_classes;

public interface IGameApplication
{
    public void SetBackgroundColor(Color color);
    public void SetVSync(bool enabled);

    public bool ClearBackground { get; set; }
    public float WindowAspectRatio { get; }

    public RenderSettings Settings { get; set; }
    public ShadingModels ShadingModel { get; set; }
    public GlobalRenderSettings GlobalSettings { get; set; }


    public event Action<float>? ChangedAspectRatio;
}
