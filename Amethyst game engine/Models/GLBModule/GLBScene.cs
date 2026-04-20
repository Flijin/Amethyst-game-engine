namespace Amethyst_game_engine.Models.GLBModule;

public class GLBScene
{
    private readonly List<GLBModel> _models;

    public List<GLBModel> Models => _models;
    public int ModelsCount => _models.Count;

    public string? SceneName { get; internal set; }

    internal GLBScene(List<GLBModel> models)
    {
        _models = models;
    }
}
