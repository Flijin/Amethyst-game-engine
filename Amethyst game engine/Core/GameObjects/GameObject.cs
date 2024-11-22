using Amethyst_game_engine.Models;
using Amethyst_game_engine.Render;

namespace Amethyst_game_engine.Core.GameObjects;

public abstract class GameObject : DrawableObject
{
    private protected readonly Mesh[] _meshes;
    private protected readonly Shader _activeShader;
    private protected bool _useCamera;

    private protected GameObject(Mesh[] meshes, bool useCamera, int shaderID)
    {
        _meshes = meshes;
        _activeShader = ShadersCollection.shaders[shaderID];
        _useCamera = useCamera;
    }

    internal sealed override void UploadFromMemory()
    {
        foreach (var mesh in _meshes)
        {
            mesh.Dispose();
        }
    }
}
