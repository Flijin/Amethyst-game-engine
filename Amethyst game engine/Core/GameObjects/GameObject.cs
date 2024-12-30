using Amethyst_game_engine.Models;
using Amethyst_game_engine.Render;

namespace Amethyst_game_engine.Core.GameObjects;

public abstract class GameObject : DrawableObject, IDisposable
{
    private bool _disposed = false;

    private protected Shader _activeShader;
    private protected bool _useCamera;
    private protected readonly Mesh[] _meshes;

    private protected int _modelProfile;
    private protected int _modelValidate;

    private int _renderFilter = 64;

    private protected GameObject(Mesh[] meshes, bool useCamera, int modelProfile, int modelValidate)
    {
        _meshes = meshes;
        _useCamera = useCamera;
        _modelValidate = modelValidate;
        _modelProfile = modelProfile;
        _activeShader = ShadersCollection.GetShader((modelProfile & (int)Window.RenderProps) | modelValidate);
    }

    ~GameObject()
    {
        if (_disposed == false)
            SystemSettings.PrintErrorMessage("Warning. The Dispose method was not called, RAM memory leak");
    }

    public void ChangeRenderSettings(RenderSettings settings)
    {
        _renderFilter = (int)settings;
        _activeShader = ShadersCollection.GetShader((_modelProfile & _renderFilter & (int)Window.RenderProps) | _modelValidate);
    }

    internal void ChangeGlobalRenderSettings()
    {
        _activeShader = ShadersCollection.GetShader((_modelProfile & _renderFilter & (int)Window.RenderProps) | _modelValidate);
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
