using System.Diagnostics.CodeAnalysis;
using Amethyst_game_engine.Core.Render;
using Amethyst_game_engine.Core.Render.Components;
using Amethyst_game_engine.Core.Render.Settings;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core.GameObjects.Components;

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

    private RenderSettings _currentSettings = RenderSettings.All;

    public Material Material { get; set; } = new();
    public RenderSettings SettingsFromModel { get; set; }
    public List<int> GLBuffers { get; } = [];


    [MemberNotNull(nameof(activeShader))]
    public void BuildShader(ShaderBuildingProps props, RenderSettings global)
    {
        _currentSettings = props.RenderSettings;

        props = ValudateFlags(props);
        props.RenderSettings &= SettingsFromModel & global;

        activeShader = ShadersPool.GetShader(props);
    }

    [MemberNotNull(nameof(activeShader))]
    public void UpdateShader(ShaderBuildingProps props)
    {
        props = ValudateFlags(props);
        props.RenderSettings &= _currentSettings & SettingsFromModel;

        activeShader = ShadersPool.GetShader(props);
    }

    public void DrawPrimitive(Vector3 cameraPos)
    {
        GL.BindVertexArray(_vao);

        SetTextures();
        SetFactors();

        if (activeShader.Props.ShadingModel != ShadingModels.Unlit)
            activeShader.SetVector3("_cameraPos", cameraPos);

        if (_isIndexedGeometry)
            GL.DrawElements(_mode, _count, _drawElementsType, 0);
        else
            GL.DrawArrays(_mode, 0, _count);
    }

    private static ShaderBuildingProps ValudateFlags(ShaderBuildingProps props)
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

        return props;
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

    private void SetTextures()
    {
        foreach (var texture in Material.textures)
        {
            if (texture is not null && (texture.key & activeShader.Props.RenderSettings) != 0)
            {
                TextureActivator.UseTexture(texture, activeShader);
            }
        }
    }

    public void Dispose()
    {
        foreach (var buffer in GLBuffers)
            GL.DeleteBuffer(buffer);

        foreach (var texture in Material.textures)
        {
            if (texture is not null)
                GL.DeleteTexture(texture.textureHandle);
        }
    }
}
