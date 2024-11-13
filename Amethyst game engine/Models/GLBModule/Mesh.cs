namespace Amethyst_game_engine.Models.GLBModule;

internal struct Mesh
{
    public int[] VAO { get; }
    public int[] VBO { get; }

    public float[,] Matrix { get; set; } =
    {
        { 1, 0, 0, 0 },
        { 0, 1, 0, 0 },
        { 0, 0, 1, 0 },
        { 0, 0, 0, 1 }
    };

    internal Mesh(Primitive[] primitives)
    {
        Primitives = primitives;
    }
}
