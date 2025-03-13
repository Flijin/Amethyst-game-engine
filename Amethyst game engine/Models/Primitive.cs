using Amethyst_game_engine.Core;
using Amethyst_game_engine.Render;
using OpenTK.Graphics.ES20;
using OpenTK.Mathematics;

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
        activeShader.SetVector3("_baseColorFactor", new Vector3(_material.BaseColorFactor.R / 255f, _material.BaseColorFactor.G / 255f, _material.BaseColorFactor.B / 255f));

        GL.DrawArrays(mode, 0, count);
    }
}
