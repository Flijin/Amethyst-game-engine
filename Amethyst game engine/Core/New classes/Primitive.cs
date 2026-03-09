using System.Diagnostics.CodeAnalysis;
using Amethyst_game_engine.Render;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core.New_classes;

internal sealed class Primitive(int vao, Primitive.Options options) : IDisposable
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

    public Material Material { get; set; }
    public RenderSettings ImportedFromModel { get; set; }
    private RenderSettings _currentRenderState;

    [AllowNull]
    public int[] Buffers { get; set; }

    [MemberNotNull(nameof(activeShader))]
    public void BuildShader(ShaderBuildingProps props)
    {
        var gourandSettings = RenderSettings.NormalMap |
                              RenderSettings.OcclusionMap |
                              RenderSettings.EmissiveMap |
                              RenderSettings.UseNormalScale |
                              RenderSettings.UseOcclusionStrength;

        if (props.ShadingModel != ShadingModels.PBR_MetallicRoughness)
        {
            props.RenderSettings &= ~(RenderSettings.MetallicRoughnessMap |
                                      RenderSettings.MetallicFactor |
                                      RenderSettings.RoughnessFactor);
        }

        if (props.ShadingModel == ShadingModels.Gouraud)
        {
            props.RenderSettings &= ~gourandSettings;
        }

        if (props.ShadingModel == ShadingModels.Unlit)
        {
            props.RenderSettings &= ~(gourandSettings | RenderSettings.EmissiveFactor);
        }

        _currentRenderState = props.RenderSettings & ImportedFromModel;
        props.RenderSettings = _currentRenderState;

        activeShader = ShadersPool.GetShader(props);
    }

    public void DrawPrimitive(Vector3 cameraPos)
    {
        activeShader.Use();
        GL.BindVertexArray(_vao);
        RenderSettings activeShaderSettings = activeShader.Props.RenderSettings;

        SetTextures(activeShaderSettings);

        if ((activeShaderSettings & RenderSettings.Lighting) != 0)
            activeShader.SetVector3("_cameraPos", cameraPos);

        SetFactors();

        if (_isIndexedGeometry)
            GL.DrawElements(_mode, _count, _drawElementsType, 0);
        else
            GL.DrawArrays(_mode, 0, _count);
    }

    private void SetFactors()
    {
        if ((activeShader.Props.RenderSettings & RenderSettings.BaseColorFactor) != 0)
            activeShader.SetVector4("_baseColorFactor", Material.BaseColorFactor.ConvertColorToVector4());

        if ((activeShader.Props.RenderSettings & RenderSettings.EmissiveFactor) != 0)
            activeShader.SetVector3("_emissiveFactor", Material.BaseColorFactor.ConvertColorToVector3());

        if ((activeShader.Props.RenderSettings & RenderSettings.MetallicFactor) != 0)
            activeShader.SetFloat("_metallicFactor", Material.MetallicFactor);

        if ((activeShader.Props.RenderSettings & RenderSettings.RoughnessFactor) != 0)
            activeShader.SetFloat("_roughnessFactor", Material.RoughnessFactor);
    }

    private void SetTextures(RenderSettings activeShaderSettings)
    {
        var currentTextures = Material.textures;

        foreach (var texture in currentTextures)
        {
            if (texture != null && (texture.textureSetting & activeShaderSettings) != 0)
            {
                TextureActivator.UseTexture(texture, activeShader);
            }
        }
    }

    public void Dispose()
    {
        foreach (var buffer in Buffers)
            GL.DeleteBuffer(buffer);

        foreach (var texture in Material.textures)
        {
            if (texture != null)
                GL.DeleteTexture(texture.textureHandle);
        }
    }
}