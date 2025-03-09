using Amethyst_game_engine.CameraModules;
using Amethyst_game_engine.Core;
using Amethyst_game_engine.Render;

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
    public int mode = 4;
    public bool isIndexedGeometry = false;

    public required Material Material
    {
        readonly get => _material;

        set
        {
            _material = value;
            activeShader = ShadersCollection.GetShader(value.MaterialKey);
        }
    }

    public void RebuildShader(uint renderSettings, uint modelSettings)
    {
        activeShader = ShadersCollection.GetShader(_material.MaterialKey & renderSettings | modelSettings);
    }

    public unsafe void DrawPrimitive(float* viewMatrix, float* projectionMatrix)
    {
        activeShader.SetMatrix4("model", ModelMatrix);
        activeShader.SetMatrix4("view", viewMatrix);
        activeShader.SetMatrix4("projection", projectionMatrix);

        GL.DrawArrays((PrimitiveType)primitive.mode, 0, primitive.count);
    }
}
