using System.Runtime.InteropServices;

namespace Amethyst_game_engine.Core.Light;

internal static class LightManager
{
    public static int MaxDirectionLights { get; private set; }

    public static int MaxPointLights { get; private set; }

    public static int MaxSpotlights { get; private set; }


    public static void SetLimitsOfLightSourses(int blockSize)
    {
        MaxDirectionLights = blockSize / Marshal.SizeOf<DirectionalLight>() - 1;
        MaxPointLights = blockSize / Marshal.SizeOf<PointLight>() - 1;
        MaxSpotlights = blockSize / Marshal.SizeOf<Spotlight>() - 1;
    }
}
