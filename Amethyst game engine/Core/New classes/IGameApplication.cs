namespace Amethyst_game_engine.Core.New_classes;

public interface IGameApplication
{
    public void SetBackgroundColor(Color color);
    public void SetVSync(bool enabled);

    public bool ClearBackground { get; set; }
    public float WindowAspectRatio { get; }


    public event Action<float>? ChangedAspectRatio;
}
