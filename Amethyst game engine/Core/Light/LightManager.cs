using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Amethyst_game_engine.Core.Light;

internal static class LightManager
{
    public static int MaxDirectionLights { get; private set; }

    public static int MaxPointLights { get; private set; }

    public static int MaxSpotLights { get; private set; }


    public static void SetLimitsOfLightSourses(int blockSize)
    {
        MaxDirectionLights = blockSize / Marshal.SizeOf<DirectionalLight>() - 1;
        MaxPointLights = blockSize / Marshal.SizeOf<PointLight>() - 1;
        MaxSpotLights = blockSize / Marshal.SizeOf<Spotlight>() - 1;
    }

    public static string GetDefines()
    {
        StringBuilder builder = new();

        builder.AppendLine($"#define DIRECTIONAL_LIGHTS_COUNT {MaxDirectionLights}");
        builder.AppendLine($"#define POINT_LIGHTS_COUNT {MaxPointLights}");
        builder.AppendLine($"#define SPOT_LIGHT_COUNT {MaxSpotLights}");

        return builder.ToString();
    }
}
