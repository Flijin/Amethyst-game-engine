using Amethyst_game_engine.CameraModule;

namespace Amethyst_game_engine.Core.New_classes;

public sealed class CameraManager : IDisposable
{
    private readonly List<Camera> _cameras = [];

    public IReadOnlyList<Camera> Cameras => _cameras;
    public int CameraCount => _cameras.Count;

    internal void UpdateAspectRatio(float aspectRatio)
    {
        foreach (var camera in _cameras)
        {
            camera.AspectRatio = aspectRatio;
        }
    }

    public bool AddCamera(Camera cam)
    {
        if (_cameras.Contains(cam))
        {
            System.PrintMessage("Warning. This camera is already exists", MessageTypes.WarningMessage);
            return false;
        }

        _cameras.Add(cam);
        return true;
    }

    public int RemoveCamera(Predicate<Camera> condition)
    {
        return _cameras.RemoveAll(condition);
    }

    public bool RemoveCameraByIndex(int index)
    {
        if (index >= 0 && index < _cameras.Count)
        {
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

    public Camera? GetCameraByIndex(int index)
    {
        if (index >= 0 && index < _cameras.Count)
            return _cameras[index];

        return null;
    }

    public void Clear()
    {
        foreach (var camera in _cameras)
            camera.Dispose();

        _cameras.Clear();
    }

    public void Dispose()
    {
        foreach (var camera in _cameras)
        {
            camera.Dispose();
        }

        _cameras.Clear();
    }
}
