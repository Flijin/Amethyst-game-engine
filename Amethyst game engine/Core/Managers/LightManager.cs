using Amethyst_game_engine.Core.Light;

namespace Amethyst_game_engine.Core.Managers;

public sealed class LightManager : IDisposable
{
    public event Action<DirectionalLight>? DirLightAdded;
    public event Action<DirectionalLight>? DirLightRemoved;
    public event Action<DirectionalLight>? DirLightUpdated;

    public event Action<PointLight>? PointLightAdded;
    public event Action<PointLight>? PointLightRemoved;
    public event Action<PointLight>? PointLightUpdated;

    public event Action<Spotlight>? SpotlightAdded;
    public event Action<Spotlight>? SpotlightRemoved;
    public event Action<Spotlight>? SpotlightUpdated;

    public event Action? OnClear;

    private readonly List<DirectionalLight> _dirLights = [];
    private readonly List<PointLight> _pointLights = [];
    private readonly List<Spotlight> _spotlights = [];

    private readonly ShaderStorageBufferManager<DirectionalLightData> _dirLightsManager = new(2, 0);
    private readonly ShaderStorageBufferManager<PointLightData> _pointLightsManager = new(64, 1);
    private readonly ShaderStorageBufferManager<SpotlightData> _spotlightsManager = new(64, 2);

    public IReadOnlyList<DirectionalLight> DirectionalLights => _dirLights;
    public IReadOnlyList<PointLight> PointLights => _pointLights;
    public IReadOnlyList<Spotlight> Spotlights => _spotlights;

    public void UpdateDirectionalLightAt(DirectionalLight light, int index)
    {
        DirLightUpdated?.Invoke(_dirLights[index]);
        _dirLights[index] = light;
        _dirLightsManager.UpdateLight(light.GetLightData(), index);
    }

    public void UpdatePointLightAt(PointLight light, int index)
    {
        PointLightUpdated?.Invoke(_pointLights[index]);
        _pointLights[index] = light;
        _pointLightsManager.UpdateLight(light.GetLightData(), index);
    }

    public void UpdateSpotlightAt(Spotlight light, int index)
    {
        SpotlightUpdated?.Invoke(_spotlights[index]);
        _spotlights[index] = light;
        _spotlightsManager.UpdateLight(light.GetLightData(), index);
    }

    public void UpdateDirectionalLights(DirectionalLight light, Predicate<DirectionalLight> condition)
    {
        for (int i = 0; i < _dirLights.Count; i++)
        {
            if (condition(_dirLights[i]))
            {
                DirLightUpdated?.Invoke(_dirLights[i]);
                _dirLights[i] = light;
                _dirLightsManager.UpdateLight(light.GetLightData(), i);
            }
        }
    }

    public void UpdatePointLights(PointLight light, Predicate<PointLight> condition)
    {
        for (int i = 0; i < _pointLights.Count; i++)
        {
            if (condition(_pointLights[i]))
            {
                PointLightUpdated?.Invoke(_pointLights[i]);
                _pointLights[i] = light;
                _pointLightsManager.UpdateLight(light.GetLightData(), i);
            }
        }
    }

    public void UpdateSpotlights(Spotlight light, Predicate<Spotlight> condition)
    {
        for (int i = 0; i < _spotlights.Count; i++)
        {
            if (condition(_spotlights[i]))
            {
                SpotlightUpdated?.Invoke(_spotlights[i]);
                _spotlights[i] = light;
                _spotlightsManager.UpdateLight(light.GetLightData(), i);
            }
        }
    }

    public void AddDirectionalLight(DirectionalLight light)
    {
        _dirLights.Add(light);
        _dirLightsManager.AddLight(light.GetLightData());
        DirLightAdded?.Invoke(light);
    }

    public void AddPointLight(PointLight light)
    {
        _pointLights.Add(light);
        _pointLightsManager.AddLight(light.GetLightData());
        PointLightAdded?.Invoke(light);
    }

    public void AddSpotlight(Spotlight light)
    {
        _spotlights.Add(light);
        _spotlightsManager.AddLight(light.GetLightData());
        SpotlightAdded?.Invoke(light);
    }

    public int RemoveDirectionalLights(Predicate<DirectionalLight> condition)
    {
        for (int i = _dirLights.Count - 1; i >= 0; i--)
        {
            if (condition(_dirLights[i]))
            {
                DirLightRemoved?.Invoke(_dirLights[i]);
                _dirLightsManager.RemoveLight(i);
            }
        }

        return _dirLights.RemoveAll(condition);
    }

    public int RemovePointLights(Predicate<PointLight> condition)
    {
        for (int i = _pointLights.Count - 1; i >= 0; i--)
        {
            if (condition(_pointLights[i]))
            {
                PointLightRemoved?.Invoke(_pointLights[i]);
                _spotlightsManager.RemoveLight(i);
            }
        }

        return _pointLights.RemoveAll(condition);
    }

    public int RemoveSpotlights(Predicate<Spotlight> condition)
    {
        for (int i = _spotlights.Count - 1; i >= 0; i--)
        {
            if (condition(_spotlights[i]))
            {
                SpotlightRemoved?.Invoke(_spotlights[i]);
                _spotlightsManager.RemoveLight(i);
            }
        }

        return _spotlights.RemoveAll(condition);
    }

    public bool RemoveDirectionalLightAt(int i)
    {
        if (i >= 0 && i < _dirLights.Count)
        {
            DirLightRemoved?.Invoke(_dirLights[i]);
            _dirLights.RemoveAt(i);
            _dirLightsManager.RemoveLight(i);

            return true;
        }

        return false;
    }

    public bool RemovePointLightAt(int i)
    {
        if (i >= 0 && i < _pointLights.Count)
        {
            PointLightRemoved?.Invoke(_pointLights[i]);
            _pointLights.RemoveAt(i);
            _pointLightsManager.RemoveLight(i);

            return true;
        }

        return false;
    }

    public bool RemoveSpotlightAt(int i)
    {
        if (i >= 0 && i < _spotlights.Count)
        {
            SpotlightRemoved?.Invoke(_spotlights[i]);
            _spotlights.RemoveAt(i);
            _spotlightsManager.RemoveLight(i);

            return true;
        }

        return false;
    }

    public IEnumerable<DirectionalLight> FindDirectionalLights(Predicate<DirectionalLight> condition)
    {
        foreach (var light in _dirLights)
        {
            if (condition(light))
                yield return light;
        }
    }

    public IEnumerable<PointLight> FindPointLights(Predicate<PointLight> condition)
    {
        foreach (var light in _pointLights)
        {
            if (condition(light))
                yield return light;
        }
    }

    public IEnumerable<Spotlight> FindSpotlights(Predicate<Spotlight> condition)
    {
        foreach (var light in _spotlights)
        {
            if (condition(light))
                yield return light;
        }
    }

    public DirectionalLight? GetDirectionalLightAt(int index)
    {
        if (index >= 0 && index < _dirLights.Count)
            return _dirLights[index];
        else
            return null;
    }

    public PointLight? GetPointLightAt(int index)
    {
        if (index >= 0 && index < _pointLights.Count)
            return _pointLights[index];
        else
            return null;
    }

    public Spotlight? GetSpotlightAt(int index)
    {
        if (index >= 0 && index < _spotlights.Count)
            return _spotlights[index];
        else
            return null;
    }

    public void Clear()
    {
        _dirLights.Clear();
        _pointLights.Clear();
        _spotlights.Clear();

        _dirLightsManager.Clear();
        _pointLightsManager.Clear();
        _spotlightsManager.Clear();

        OnClear?.Invoke();
    }

    internal void Cleanup()
    {
        _dirLightsManager.Dispose();
        _pointLightsManager.Dispose();
        _spotlightsManager.Dispose();

        DirLightAdded = null;
        DirLightRemoved = null;
        DirLightUpdated = null;

        PointLightAdded = null;
        PointLightRemoved = null;
        PointLightUpdated = null;

        SpotlightAdded = null;
        SpotlightRemoved = null;
        SpotlightUpdated = null;
    }

    void IDisposable.Dispose()
    {
        Cleanup();
    }
}
