namespace Game_Engine.Core.Models.GLBModule;

internal class GLBScene(GLBModel[] models)
{
    private readonly GLBModel[] _models = models;

    public string Name { get; set; } = "None";
    public int ModelsCount { get; } = models.Length;
    public float[,] SceneMatrix { get; set; } =
    {
        { 1, 0, 0, 0 },
        { 0, 1, 0, 0 },
        { 0, 0, 1, 0 },
        { 0, 0, 0, 1 },
    };

    public GLBModel GetModelByIndex(int index) => _models[index];
}
