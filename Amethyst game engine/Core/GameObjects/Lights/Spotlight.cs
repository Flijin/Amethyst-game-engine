using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core.GameObjects.Lights;

class Spotlight
{
    private float _innerCutoff;
    private float _outerCutoff;

    public float InnerCutoff
    {
        get => _innerCutoff;

        set => _innerCutoff = Mathematics.Clamp(value, 0, 180);
    }

    public float OuterCutoff
    {
        get => _outerCutoff;

        set => _outerCutoff = Mathematics.Clamp(value, 0, 180);
    }

    public Vector3 Position { get; set; }

    public Spotlight(Color color, Vector3 direction, Vector3 position, float innerCutoff, float outerCutoff)
    {
        Position = position;
        InnerCutoff = innerCutoff;
        OuterCutoff = outerCutoff;
    }
}
