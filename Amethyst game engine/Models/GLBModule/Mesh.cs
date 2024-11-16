namespace Amethyst_game_engine.Models.GLBModule;

internal readonly struct Mesh
{
    public readonly Primitive[] primitives;
    public readonly int[] buffers;

    public readonly float[,] matrix =
    {
        { 1, 0, 0, 0 },
        { 0, 1, 0, 0 },
        { 0, 0, 1, 0 },
        { 0, 0, 0, 1 }
    };

    public Mesh(Primitive[] primitives, int[] buffers, float[,]? matrix)
    {
        this.primitives = primitives;
        this.buffers = buffers;

        if (matrix is not null)
            this.matrix = matrix;
    }
}
