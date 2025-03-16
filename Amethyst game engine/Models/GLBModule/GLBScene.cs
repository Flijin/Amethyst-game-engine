namespace Amethyst_game_engine.Models.GLBModule;

public struct GLBScene
{
    private GLBModel[] _models;

    public string Name { get; internal set; } = "None";
    public int ModelsCount { get; private set; }

    public GLBModel[] Models
    {
        readonly get => _models;

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

    public readonly GLBModel GetModelByIndex(int index) => _models[index];

    public readonly GLBModel? GetModelByName(string name) => _models.FirstOrDefault(obj => obj.Name == name);
}
