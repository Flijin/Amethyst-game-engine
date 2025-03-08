using Amethyst_game_engine.Core;
using Amethyst_game_engine.Render;

namespace Amethyst_game_engine.Models;

internal struct Primitive(int vao)
{
    public readonly int vao = vao;

    public int count = 0;
    public int componentType = 5126;
    public int mode = 4;
    public bool isIndexedGeometry = false;

    public Material material = Material.NoneMaterial;

    public Shader activeShader;
}
