namespace Amethyst_game_engine.Models.GLBModule;

public class GLBScene
{
    private GLBModel[] _models;

    public string Name { get; set; } = "None";
    public int ModelsCount { get; private set; }

    public GLBModel[] Models
    {
        get => _models;

        set
        {
            _models = value;
            ModelsCount = value.Length;
        }
    }

    public GLBScene() : this([]) { }

    public GLBScene(GLBModel[] models)
    {
        _models = models;
        ModelsCount = _models.Length;
    }

    public GLBModel GetModelByIndex(int index) => _models[index];
}
