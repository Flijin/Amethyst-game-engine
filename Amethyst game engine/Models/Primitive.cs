using Amethyst_game_engine.Core;
using Amethyst_game_engine.Render;
using OpenTK.Graphics.ES30;

namespace Amethyst_game_engine.Models;

internal struct Primitive(int vao)
{
#nullable disable
    public Shader activeShader;
    private Material _material;
#nullable restore

    public readonly int vao = vao;

    public int count = 0;
    public int componentType = 5126;
    public PrimitiveType mode = PrimitiveType.Triangles;
    public bool isIndexedGeometry = false;

    public required Material Material
    {
        readonly get => _material;

        set
        {
            _material = value;
            activeShader = ShadersPool.GetShader(value.materialKey);
        }
    }

    public void BuildShader(uint renderSettings, uint modelSettings)
    {
        activeShader = ShadersPool.GetShader(_material.materialKey & renderSettings | modelSettings);
    }

    public readonly void DrawPrimitive()
    {
        activeShader.SetFloats(_material.uniforms_float);
        activeShader.SetInts(_material.uniforms_int);

        GL.DrawArrays(mode, 0, count);
    }
}
