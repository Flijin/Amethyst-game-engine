using Amethyst_game_engine.Models;
using Amethyst_game_engine.Render;

namespace Amethyst_game_engine.Core.GameObjects;

public abstract class GameObject : DrawableObject, IDisposable
{
    private bool _disposed = false;

    private protected readonly Mesh[] _meshes;
    private protected readonly Shader _activeShader;
    private protected bool _useCamera;

    private protected GameObject(Mesh[] meshes, bool useCamera, int shaderID)
    {
        _meshes = meshes;
        _activeShader = ShadersCollection.shaders[shaderID];
        _useCamera = useCamera;
    }

    ~GameObject()
    {
        if (_disposed == false)
            SystemSettings.PrintErrorMessage("Warning. The Dispose method was not called, RAM memory leak");
    }

    public override sealed void Dispose()
    {
        if (_disposed == false)
        {
            base.Dispose();

            foreach (var mesh in _meshes)
            {
                mesh.Dispose();
            }

            GC.SuppressFinalize(this);

            _disposed = true;
        }
    }
}
