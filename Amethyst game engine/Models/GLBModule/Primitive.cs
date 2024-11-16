namespace Amethyst_game_engine.Models.GLBModule;

internal readonly struct Primitive(int vao, int componentType, int count, int mode, int textureHandle, bool isIndexedGeometry)
{
    public readonly int vao = vao;
    public readonly int componentType = componentType;
    public readonly int count = count;
    public readonly int mode = mode;
    public readonly int textureHandle = textureHandle;
    public readonly bool isIndexedGeometry = isIndexedGeometry;
}
