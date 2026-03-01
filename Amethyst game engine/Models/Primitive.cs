using System.Diagnostics.CodeAnalysis;
using Amethyst_game_engine.Core;
using Amethyst_game_engine.Render;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Models;

internal sealed class Primitive(int vao, Material material, Primitive.Options options)
{
    public struct Options
    {
        public int Count { get; set; }
        public DrawElementsType DrawElementsType { get; set; }
        public PrimitiveType Mode { get; set; }
        public bool IsIndexedGeometry { get; set; }
    }

    [AllowNull]
    public Shader activeShader;

    private readonly int _vao = vao;

    private readonly int _count = options.Count;
    private readonly DrawElementsType _drawElementsType = options.DrawElementsType;
    private readonly PrimitiveType _mode = options.Mode;
    private readonly bool _isIndexedGeometry = options.IsIndexedGeometry;

    public Material Material { get; set; } = material;

    [MemberNotNull(nameof(activeShader))]
    public void BuildShader(ShaderBuildingProps props)
    {
        props.RenderSettings &= Material.materialKey;
        activeShader = ShadersPool.GetShader(props);
    }

    public void DrawPrimitive(Vector3 cameraPos, int[] ssbo)
    {
        activeShader.Use();
        GL.BindVertexArray(_vao);
        RenderSettings activeShaderSettings = activeShader.Props.RenderSettings;

        SetTextures(activeShaderSettings);

        if ((activeShaderSettings & RenderSettings.Lighting) != 0)
            activeShader.SetVector3("_cameraPos", cameraPos);

        foreach (var factor in Material.factors)
        {
            if ((factor.settings & activeShaderSettings) == 0 || factor.factor is null)
                continue;

            if (UniformsCache.TryGetUniformName(factor.settings, out string? name) == false)
                continue;

            switch (factor.factor)
            {
                case float floatVar:
                        activeShader.SetFloat(name!, floatVar);
                    break;
                case Vector3 vector3Var:
                        activeShader.SetVector3(name!, vector3Var);
                    break;
                case Vector4 vector4Var:
                        activeShader.SetVector4(name!, vector4Var);
                    break;
            }
        }

        UseSSBO(ssbo, activeShaderSettings);

        if (_isIndexedGeometry)
            GL.DrawElements(_mode, _count, _drawElementsType, 0);
        else
            GL.DrawArrays(_mode, 0, _count);
    }

    private static void UseSSBO(int[] ssboArray, RenderSettings settings)
    {
        if ((settings & RenderSettings.Lighting) == 0)
            return;

        for (int i = 0; i < ssboArray.Length; i++)
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, i, ssboArray[i]);
    }

    private void SetTextures(RenderSettings activeShaderSettings)
    {
        var currentTextures = Material.textures;

        foreach (var texture in currentTextures)
        {
            if ((texture.settings & activeShaderSettings) != 0 && texture.texture != null)
            {
                TextureActivator.UseTexture(texture.texture, activeShader);
            }
        }
    }
}