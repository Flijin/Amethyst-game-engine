using System.Diagnostics.CodeAnalysis;
using Amethyst_game_engine.CameraModule;
using Amethyst_game_engine.Core;
using Amethyst_game_engine.Render;
using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Models;

internal struct Primitive
{
    [AllowNull]
    public Shader activeShader;

    public readonly int vao;

    public int count = 0;
    public int drawElementsType = 5125;
    public PrimitiveType mode = PrimitiveType.Triangles;
    public bool isIndexedGeometry = false;

    private bool _useLightning;
    private RenderSettings _primitiveSettings;
    private RenderSettings _currentRenderKeys;

    public Material Material { get; set; }

    public Primitive(int vao, Material material)
    {
        this.vao = vao;
        Material = material;

        CalculateRenderKeys();
    }

    private void CalculateRenderKeys() => _currentRenderKeys = _primitiveSettings & Material.materialKey;

    public void BuildShader(bool useMeshMatrix)
    {
        CalculateRenderKeys();

        //activeShader = ShadersPool.GetShader();
        //if ((_currentRenderKeys & (1 << 1)) != 0)
        //    _useLightning = true;
        //else
        //    _useLightning = false;

        //activeShader = ShadersPool.GetShader(_currentRenderKeys, (uint)Window.ShadingModel);
    }

    public unsafe readonly void DrawPrimitive(Vector3 cameraPos)
    {
        activeShader.Use();

        var renderMask = _currentRenderKeys & Window.RenderKeys;
        var shadingModel = Window.ShadingModel;

        GL.BindVertexArray(vao);


        if (isIndexedGeometry)
            GL.DrawElements(mode, count, (DrawElementsType)drawElementsType, 0);
        else
            GL.DrawArrays(mode, 0, count);
    }

//# ifdef USE_MESH_MATRIX
//    uniform mat4 meshMatrix;
//#endif

//uniform mat4 modelMatrix;
//uniform mat4 viewMatrix;
//uniform mat4 projectionMatrix;

    private void SetUniforms()
    {

    }

    private void UseTextures()
    {

    }
}


//if (_useLightning)
//        {
//            if (Window.ShadingModel != ShadingModels.lAMBERTIAN_SHADING_MODEL)
//                activeShader.SetVector3("_cameraPos", cameraPos);

//            activeShader.SetInt("_numDirectionalLights", countOfDirLights);
//            activeShader.SetInt("_numPointLights", countOfPointLights);
//            activeShader.SetInt("_numSpotlights", countOfSpotLights);
//        }

//        activeShader.SetFloats(_uniforms_float);
//activeShader.SetInts(_uniforms_int);

//if (_baseColorFactor.isNoneColor == false)
//    activeShader.SetVector4("_baseColorFactor", _baseColorFactor.GetColorInVectorForm());

//ShaderDataTransmitter.BindTexturesToUnits(_usedTextureUnits);

//if (isIndexedGeometry)
//    GL.DrawElements(mode, count, (DrawElementsType)drawElementsType, 0);
//else
//    GL.DrawArrays(mode, 0, count);