using Amethyst_game_engine.Core.Light;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core.New_classes;

public class DirectionalLight(string tag)
{
    public string Tag { get; } = tag;

    public Vector3 Direction { get; set; } = Vector3.Zero;
    public Vector3 Color { get; set; } = Core.Color.White.ConvertColorToVector3();
    public float Intensity { get; set; } = 1.0f;
    
    public DirectionalLight() : this(string.Empty) { }

    internal DirectionalLightData GetLightData()
    {
        return new DirectionalLightData()
        {
            direction = Direction,
            color = Color,
            intensity = Intensity
        };
    }

}
