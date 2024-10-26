namespace Game_Engine.Core.Models.GLBModule;

internal class GLBScene(GLBModel[] models, string name)
{
    private readonly GLBModel[] _models = models;

    public string Name { get; } = name;
    public int ModelsCount { get; } = models.Length;

    public GLBModel GetModelByIndex(int index) => _models[index];
}
