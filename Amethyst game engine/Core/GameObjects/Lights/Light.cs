namespace Amethyst_game_engine.Core.GameObjects.Lights;

internal struct Light
{
    public Color LightColor { get; set; }

    public Light()
    {
        LightColor = Color.White;
    }
}
