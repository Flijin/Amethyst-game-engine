namespace Amethyst_game_engine.Core.GameObjects.Components;

internal struct Sampler
{
    public static Sampler DefaultSampler => new()
    {
        MagFilter = 9729,
        MinFilter = 9987,
        WrapS = 10497,
        WrapT = 10497
    };

    public int MagFilter { get; set; }
    public int MinFilter { get; set; }
    public int WrapS {  get; set; }
    public int WrapT { get; set; }
}
