using Amethyst_game_engine.CameraModule;
using Amethyst_game_engine.Core.GameObjects;
using Amethyst_game_engine.Core.Light;

namespace Amethyst_game_engine.Core.New_classes;

public sealed class LightManager
{
    private readonly List<DirectionalLight> _dirLights = [];
    private readonly List<PointLight> _pointLights = [];
    private readonly List<Spotlight> _spotlights = [];

    public int DirectionalLightsCount => _dirLights.Count;
    public int PointLightsCount => _pointLights.Count;
    public int SpotlightsCount => _spotlights.Count;
    public int TotalLightsCount => _dirLights.Count + _pointLights.Count + _spotlights.Count;

    public IReadOnlyList<DirectionalLight> DirectionalLights => _dirLights;
    public IReadOnlyList<PointLight> PointLights => _pointLights;
    public IReadOnlyList<Spotlight> Spotlights => _spotlights;

    internal IReadOnlyList<DirectionalLightData> DirectionalLightData => [.. _dirLights.Select(light => light.GetLightData())];
    internal IReadOnlyList<PointLightData> PointLightData => [.. _pointLights.Select(light => light.GetLightData())];
    internal IReadOnlyList<SpotlightData> SpoplightData => [.. _spotlights.Select(light => light.GetLightData())];

    public void AddDirectionalLight(DirectionalLight light) => _dirLights.Add(light);
    public void AddPointLight(PointLight light) => _pointLights.Add(light);
    public void AddSpotlight(Spotlight light) => _spotlights.Add(light);

    private List<T> GetList<T>() where T: class
    {
        if (typeof(T) == typeof(DirectionalLight) && _dirLights is List<T> dirLightsList)
            return dirLightsList;
        if (typeof(T) == typeof(PointLight) && _pointLights is List<T> pointLightsList)
            return pointLightsList;
        if (typeof(T) == typeof(Spotlight) && _spotlights is List<T> spotLightsList)
            return spotLightsList;

        System.PrintMessage($"Error. Type {typeof(T)} does not exist");
        return null!;
    }

    public int RemoveLights<T>(Predicate<T> condition) where T : class
    {
        List<T> lightList = GetList<T>();

        if (lightList is null)
            return 0;
        return lightList.RemoveAll(condition);
    }

    public bool RemoveLightByIndex<T>(int index) where T : class
    {
        List<T> lightList = GetList<T>();

        if (lightList == null)
            return false;

        if (index >= 0 && index < lightList.Count)
        {
            lightList.RemoveAt(index);
            return true;
        }

        return false;
    }

    public IEnumerable<T> FindLights<T>(Predicate<T> condition) where T: class
    {
        List<T> lightList = GetList<T>();

        if (lightList is null)
            yield break;

        foreach (var light in lightList)
        {
            if (condition(light))
                yield return light;
        }
    }

    public T? GetLightByIndex<T>(int index) where T: class
    {
        List<T> lightList = GetList<T>();

        if (lightList is null)
            return null;

        if (index >= 0 && index < lightList.Count)
            return lightList[index];
        else
            return null;
    }

    public void Clear()
    {
        _dirLights.Clear();
        _pointLights.Clear();
        _spotlights.Clear();
    }
}
