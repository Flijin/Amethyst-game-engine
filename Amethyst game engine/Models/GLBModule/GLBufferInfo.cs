namespace Amethyst_game_engine.Models.GLBModule;

internal readonly struct GLBufferInfo(int buffer, int stride, int componentType, int count, bool normalized)
{
    public readonly int buffer = buffer;
    public readonly int stride = stride;
    public readonly int componentType = componentType;
    public readonly int count = count;
    public readonly bool normalized = normalized;
}
