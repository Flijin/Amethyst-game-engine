using Amethyst_game_engine.CameraModules;
using Amethyst_game_engine.Models.GLBModule;
using Amethyst_game_engine.Render;

namespace Amethyst_game_engine.Core;

public abstract class DrawableObject : IDisposable
{
    private protected float[,] _modelMatrix;
    private protected Mesh[] _meshes;
    private protected Shader _activeShader;

    private protected DrawableObject(Mesh[] meshes, int shaderID)
    {
        _meshes = meshes;
        _activeShader = ShadersCollection.shaders[shaderID];

        _modelMatrix =
            new float[4, 4]
            {
                { 1, 0, 0, 0 },
                { 0, 1, 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 },
            };
    }

    internal abstract void DrawObject(Camera? cam);

    internal void UploadFromMemory() => Dispose();

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        foreach (var mesh in _meshes)
        {
            mesh.Dispose();
        }
    }
}
