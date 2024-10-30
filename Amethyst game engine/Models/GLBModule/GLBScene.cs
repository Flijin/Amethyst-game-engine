using Amethyst_game_engine.CameraModules;

namespace Amethyst_game_engine.Models.GLBModule;

internal class GLBScene(GLBModel[] models)
{
    private readonly GLBModel[] _models = models;

    public string Name { get; set; } = "None";
    public int ModelsCount { get; } = models.Length;
    public Camera? Camera { get; set; } = null;

    public float[,] SceneMatrix { get; set; } =
    {
        { 1, 0, 0, 0 },
        { 0, 1, 0, 0 },
        { 0, 0, 1, 0 },
        { 0, 0, 0, 1 },
    };

    public GLBModel GetModelByIndex(int index) => _models[index];
}
