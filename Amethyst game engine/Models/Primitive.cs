using System.Diagnostics.CodeAnalysis;
using Amethyst_game_engine.Core;
using Amethyst_game_engine.Render;
using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Models;

internal struct Primitive(int vao, Material material)
{
    private readonly Dictionary<string, int> _uniforms_int = [];
    private readonly Dictionary<string, float> _uniforms_float = [];
    private readonly Dictionary<TextureUnit, int> _usedTextureUnits = [];

    private bool _useLightning;

    private Color _baseColorFactor;

    [AllowNull]
    public Shader activeShader;

    public readonly int vao = vao;

    public int count = 0;
    public int drawElementsType = 5125;
    public PrimitiveType mode = PrimitiveType.Triangles;
    public bool isIndexedGeometry = false;

    public Material Material { get; set; } = material;

    public void BuildShader(uint localSettings, uint useMeshMatrixKey)
    {
        var flags = Material.materialKey & localSettings;

        if ((flags & (1 << 1)) != 0)
            _useLightning = true;
        else
            _useLightning = false;

        activeShader = ShadersPool.GetShader(flags & (uint)Window.RenderKeys | useMeshMatrixKey, (uint)Window.ShadingModel);
        LimitShaderData(localSettings);
    }

    public readonly void DrawPrimitive(Vector3 cameraPos, int countOfDirLights, int countOfPointLights, int countOfSpotLights)
    {
        if (_useLightning)
        {
            if (Window.ShadingModel != ShadingModels.lAMBERTIAN_SHADING_MODEL)
                activeShader.SetVector3("_cameraPos", cameraPos);

            activeShader.SetInt("_numDirectionalLights", countOfDirLights);
            activeShader.SetInt("_numPointLights", countOfPointLights);
            activeShader.SetInt("_numSpotlights", countOfSpotLights);
        }

        activeShader.SetFloats(_uniforms_float);
        activeShader.SetInts(_uniforms_int);

        if (_baseColorFactor.isNoneColor == false)
            activeShader.SetVector4("_baseColorFactor", _baseColorFactor.GetColorInVectorForm());

        ShaderDataTransmitter.BindTexturesToUnits(_usedTextureUnits);

        if (isIndexedGeometry)
            GL.DrawElements(mode, count, (DrawElementsType)drawElementsType, 0);
        else
            GL.DrawArrays(mode, 0, count);
    }
}
