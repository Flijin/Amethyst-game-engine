namespace Amethyst_game_engine.Models.GLBModule;

internal struct Primitive(int vao)
{
    public readonly int vao = vao;

    public int count = 0;
    public int componentType = 5126;
    public int mode = 4;
    public bool isIndexedGeometry = false;

    public readonly int[] materialsUsed = new int[5];
    public readonly int[] textureHandles = new int[5];

    public readonly Dictionary<MaterialsProperties, (float[] factor, int texture)> material = new()
    {
        { MaterialsProperties.Albedo, ([1.0f, 1.0f, 1.0f, 1.0f], -1) },
        { MaterialsProperties.MetallicRoughness, ([0.0f, 0.0f], -1) },
        { MaterialsProperties.Normal, ([1.0f], -1) },
        { MaterialsProperties.Occlusion, ([1.0f], -1) },
        { MaterialsProperties.Emissive, ([0.0f], -1) },
    };
}
