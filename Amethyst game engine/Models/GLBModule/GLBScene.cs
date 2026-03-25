namespace Amethyst_game_engine.Models.GLBModule;

public class GLBScene
{
    private readonly GLBModel[] _models;

    public GLBModel[] Models => _models;
    public int ModelsCount => _models.Length;

    public string? SceneName { get; internal set; }

    internal GLBScene(GLBModel[] models)
    {
        _models = models;
    }
}
