using Amethyst_game_engine.Core;
using Amethyst_game_engine.Render;
using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Models;

internal struct Primitive(int vao, Material material)
{
    private static readonly Dictionary<uint, string> _uniformNames = new()
    {
        [1 << 2] = "_albedoTexture",
        [1 << 3] = "_metallicRoughnessTexture",
        [1 << 4] = "_normalTexture",
        [1 << 5] = "_occlusionTexture",
        [1 << 6] = "_emissiveTexture",
        [1 << 7] = "_baseColorFactor",
        [1 << 8] = "_metallicFactor",
        [1 << 9] = "_roughnessFactor",
        [1 << 10] = "_emissiveFactor",
    };

    private static readonly Dictionary<uint, TextureUnit> _textureUnits = new()
    {
        [1 << 2] = TextureUnit.Texture0,
        [1 << 3] = TextureUnit.Texture1,
        [1 << 4] = TextureUnit.Texture2,
        [1 << 5] = TextureUnit.Texture3,
        [1 << 6] = TextureUnit.Texture4,
    };

    private readonly Dictionary<string, int> _uniforms_int = [];
    private readonly Dictionary<string, float> _uniforms_float = [];
    private readonly Dictionary<TextureUnit, int> _usedTextureUnits = [];

    private bool _useLightning;

    private Color _baseColorFactor;

#nullable disable
    public Shader activeShader;
#nullable restore

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

    private void LimitShaderData(uint renderSettings)
    {
        _uniforms_float.Clear();
        _uniforms_int.Clear();
        _usedTextureUnits.Clear();

        _baseColorFactor = Color.NoneColor;

        var startDigit = 4u;

        var flags_int = renderSettings & Material.materialKey & 0b_01111100;
        var baseColorFactor = renderSettings & Material.materialKey & 0b_10000000;
        var flags_float = renderSettings & Material.materialKey & 0b_00000111_00000000;

        var leftBorder = 1 << 10;

        while (startDigit <= leftBorder)
        {
            if (flags_int <= startDigit && (flags_int & startDigit) != 0)
            {
                var handler = (int)Material[startDigit];

                _uniforms_int.Add(_uniformNames[startDigit], (int)_textureUnits[startDigit] - (int)TextureUnit.Texture0);
                _usedTextureUnits.Add(_textureUnits[startDigit], handler);
            }
            else if (baseColorFactor == startDigit)
            {
                _baseColorFactor = Material.BaseColorFactor;
            }
            else if ((flags_float & startDigit) != 0)
            {
                _uniforms_float.Add(_uniformNames[startDigit], Material[startDigit]);
            }

            startDigit <<= 1;
        }
    }
}
