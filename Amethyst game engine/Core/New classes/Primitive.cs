using System.Diagnostics.CodeAnalysis;
using Amethyst_game_engine.Render;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core.New_classes;

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
        var systemFlags = (RenderSettings.VertexColors | RenderSettings.Lighting) & props.RenderSettings;

        if (props.ShadingModel != ShadingModels.PBR_MetallicRoughness)
        {
            props.RenderSettings &= ~(RenderSettings.MetallicRoughnessMap |
                                     RenderSettings.MetallicFactor |
                                     RenderSettings.RoughnessFactor);
        }

        props.RenderSettings = props.RenderSettings & Material.MaterialKey | systemFlags;
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

        SetFactors();

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

    private void SetFactors()
    {
        if ((activeShader.Props.RenderSettings & RenderSettings.BaseColorFactor) != 0)
            activeShader.SetVector4("_baseColorFactor", Material.BaseColorFactor.ConvertColorToVector4());

        if ((activeShader.Props.RenderSettings & RenderSettings.MetallicFactor) != 0)
            activeShader.SetFloat("_metallicFactor", Material.MetallicFactor);

        if ((activeShader.Props.RenderSettings & RenderSettings.RoughnessFactor) != 0)
            activeShader.SetFloat("_roughnessFactor", Material.RoughnessFactor);

        if ((activeShader.Props.RenderSettings & RenderSettings.EmissiveFactor) != 0)
            activeShader.SetVector3("_emissiveFactor", Material.BaseColorFactor.ConvertColorToVector3());
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