namespace Amethyst_game_engine.Models.GLBModule;

internal struct Mesh
{
    public float[] Vertices { get; }

    public float[,] Matrix { get; set; } =
    {
        { 1, 0, 0, 0 },
        { 0, 1, 0, 0 },
        { 0, 0, 1, 0 },
        { 0, 0, 0, 1 }
    };

    internal Mesh(float[] vertices)
    {
        Vertices = vertices;
    }
}
