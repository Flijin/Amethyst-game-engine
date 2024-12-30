namespace Amethyst_game_engine.Models.GLBModule;

internal struct GLBufferInfo(int buffer, int componentType)
{
    public readonly int buffer = buffer;
    public readonly int componentType = componentType;
    
    public int stride = 0;
    public int count = 0;
    public int countOfComponents = 3;
    public bool normalized = false;
}
