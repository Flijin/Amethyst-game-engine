using Amethyst_game_engine.Core;
using Amethyst_game_engine.Render;
using OpenTK.Graphics.ES30;

namespace Amethyst_game_engine.Models;

internal struct Primitive(int vao)
{
    private static readonly Dictionary<uint, string> _uniformNames = new()
    {
        [1 << 2] = "_albeloTexture_0",
        [1 << 3] = "_albeloTexture_1",
        [1 << 4] = "_albeloTexture_2",
        [1 << 5] = "_albeloTexture_3",
        [1 << 6] = "_metallicRoughnessTexture",
        [1 << 7] = "_normalTexture",
        [1 << 8] = "_occlusionTexture",
        [1 << 9] = "_emissiveTexture",
        [1 << 10] = "_baseColorFactor",
        [1 << 11] = "_metallicFactor",
        [1 << 12] = "_roughnessFactor",
        [1 << 13] = "_emissiveFactor",
    };

    private static readonly Dictionary<uint, TextureUnit> _textureUnits = new()
    {
        [1 << 2] = TextureUnit.Texture0,
        [1 << 3] = TextureUnit.Texture1,
        [1 << 4] = TextureUnit.Texture2,
        [1 << 5] = TextureUnit.Texture3,
        [1 << 6] = TextureUnit.Texture4,
        [1 << 7] = TextureUnit.Texture5,
        [1 << 8] = TextureUnit.Texture6,
        [1 << 9] = TextureUnit.Texture7
    };

    private readonly Dictionary<string, int> _uniforms_int = [];
    private readonly Dictionary<string, float> _uniforms_float = [];
    private readonly Dictionary<TextureUnit, int> _usedTextureUnits = [];

    private Color _baseColorFactor;

#nullable disable
    public Shader activeShader;
#nullable restore

    public readonly int vao = vao;

    public int count = 0;
    public int componentType = 5126;
    public PrimitiveType mode = PrimitiveType.Triangles;
    public bool isIndexedGeometry = false;
    
    public required Material Material { get; set; }

    public void BuildShader(uint renderSettings, uint useMeshMatrixKey)
    {
        activeShader = ShadersPool.GetShader(Material.materialKey & renderSettings | useMeshMatrixKey);
        LimitShaderData(renderSettings);
    }

    public readonly void DrawPrimitive()
    {
        activeShader.SetFloats(_uniforms_float);
        activeShader.SetInts(_uniforms_int);

        if (_baseColorFactor.isNoneColor == false)
            activeShader.SetVector4("_baseColorFactor", _baseColorFactor.GetColorInVectorForm());

        ShaderDataTransmitter.BindTexturesToUnits(_usedTextureUnits);

        GL.DrawArrays(mode, 0, count);
    }

    private void LimitShaderData(uint renderSettings)
    {
        _uniforms_float.Clear();
        _uniforms_int.Clear();
        _usedTextureUnits.Clear();

        _baseColorFactor = Color.NoneColor;

        var startDigit = 4u;

        var flags_int = renderSettings & Material.materialKey & 0b_00000011_11111100;
        var baseColorFactor = renderSettings & Material.materialKey & 0b_00000100_00000000;
        var flags_float = renderSettings & Material.materialKey & 0b_00111000_00000000;

        while (startDigit <= (1 << 13))
        {
            if (flags_int <= startDigit && (flags_int & startDigit) != 0)
            {
                var handler = (int)Material[startDigit];

                _uniforms_int.Add(_uniformNames[startDigit], handler);
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
