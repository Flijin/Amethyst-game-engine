using Amethyst_game_engine.Models;
using Amethyst_game_engine.Render;

namespace Amethyst_game_engine.Core.GameObjects;

public abstract class GameObject : DrawableObject
{
    private bool _disposed = false;

    private protected bool _useCamera;
    private protected readonly Mesh[] _meshes;

    private int _modelProfile;
    private int _currentRenderState = (int)RenderSettings.All;

    private protected GameObject(Mesh[] meshes, bool useCamera, int modelProfile)
    {
        _meshes = meshes;
        _useCamera = useCamera;
        _modelProfile = modelProfile;
        _activeShader = ShadersCollection.GetShader(_modelProfile & (int)Window.RenderKeys);
    }

    ~GameObject()
    {
        if (_disposed == false)
            SystemSettings.PrintErrorMessage("Warning. The Dispose method was not called, RAM memory leak");
    }

    public void ChangeRenderSettings(RenderSettings settings)
    {
        _currentRenderState = _modelProfile & (int)settings & (int)Window.RenderKeys;
        _activeShader = ShadersCollection.GetShader(_currentRenderState);
    }

    internal void ChangeGlobalRenderSettings()
    {
        _activeShader = ShadersCollection.GetShader(_currentRenderState & (int)Window.RenderKeys);
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
