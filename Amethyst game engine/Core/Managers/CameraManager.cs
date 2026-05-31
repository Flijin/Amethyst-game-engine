using Amethyst_game_engine.Core.CameraModule;
using Amethyst_game_engine.Core.Utilities;

namespace Amethyst_game_engine.Core.Managers;

public sealed class CameraManager : IDisposable
{
    public event Action<Camera>? CameraAdded;
    public event Action<Camera>? CameraRemoved;
    public event Action? OnClear;

    private readonly List<Camera> _cameras = [];

    public IReadOnlyList<Camera> Cameras => _cameras;
    public int CameraCount => _cameras.Count;

    public bool AddCamera(Camera cam)
    {
        if (_cameras.Contains(cam))
        {
            SystemCalls.PrintMessage("Warning. Camera is already exists", MessageTypes.WarningMessage);
            return false;
        }

        _cameras.Add(cam);
        CameraAdded?.Invoke(cam);

        return true;
    }

    public int RemoveCamera(Predicate<Camera> condition)
    {
        for (int i = _cameras.Count - 1; i >= 0; i--)
        {
            if (condition(_cameras[i]))
            {
                CameraRemoved?.Invoke(_cameras[i]);
                _cameras[i].Cleanup();
            }
        }

        return _cameras.RemoveAll(condition);
    }

    public bool RemoveCameraAt(int index)
    {
        if (index >= 0 && index < _cameras.Count)
        {
            CameraRemoved?.Invoke(_cameras[index]);
            _cameras.RemoveAt(index);

            return true;
        }

        return false;
    }

    public IEnumerable<Camera> FindCameras(Predicate<Camera> condition)
    {
        foreach (var camera in _cameras)
        {
            if (condition(camera))
                yield return camera;
        }
    }

    public Camera? GetCameraAt(int index)
    {
        if (index >= 0 && index < _cameras.Count)
            return _cameras[index];

        return null;
    }

    public void Clear()
    {
        Cleanup();
        _cameras.Clear();

        OnClear?.Invoke();
    }

    internal void Cleanup()
    {
        foreach (var camera in _cameras)
            camera.Cleanup();
    }

    void IDisposable.Dispose()
    {
        Cleanup();
    }
}
