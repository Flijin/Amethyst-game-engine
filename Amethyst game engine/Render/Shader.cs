#define DEBUG_MODE

using Amethyst_game_engine.Core.Light;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Amethyst_game_engine.Render;

internal class Shader : IDisposable
{
    private readonly Dictionary<string, int> _uniformLocations;

    public int Handle { get; private set; }

    public Shader(uint shaderFlags, uint shadingModel)
    {
        Handle = GL.CreateProgram();

        var vertexDescriptor = CreateAndAttachShader(ShaderType.VertexShader, Handle, shaderFlags, shadingModel);
        var fragmentDescriptor = CreateAndAttachShader(ShaderType.FragmentShader, Handle, shaderFlags, shadingModel);

        GL.LinkProgram(Handle);
        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int code);

        if (code == 0) SystemSettings.PrintMessage("Error. " + GL.GetShaderInfoLog(Handle), Core.MessageTypes.ErrorMessage);

        ClearShader(vertexDescriptor);
        ClearShader(fragmentDescriptor);

        _uniformLocations = GetUniforms();

        void ClearShader(int descriptor)
        {
            GL.DetachShader(Handle, descriptor);
            GL.DeleteShader(descriptor);
        }

        GL.GetString(StringNameIndexed.ShadingLanguageVersion, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Use() => GL.UseProgram(Handle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => GL.DeleteProgram(Handle);

    public void SetFloats(Dictionary<string, float> data)
    {
        var pairs = data.ToArray();

        for (int i = 0; i < pairs.Length; i++)
        {
            GL.Uniform1(_uniformLocations[pairs[i].Key], pairs[i].Value);
        }
    }

    public void SetInts(Dictionary<string, int> data)
    {
        var pairs = data.ToArray();

        for (int i = 0; i < pairs.Length; i++)
        {
            GL.Uniform1(_uniformLocations[pairs[i].Key], pairs[i].Value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void SetMatrix4(string name, float* matrixPtr) => GL.UniformMatrix4(_uniformLocations[name], 1, true, matrixPtr);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetFloat(string name, float value) => GL.Uniform1(_uniformLocations[name], value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetInt(string name, int value) => GL.Uniform1(_uniformLocations[name], value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetVector3(string name, Vector3 vec3) => GL.Uniform3(_uniformLocations[name], vec3);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetVector4(string name, Vector4 vec4) => GL.Uniform4(_uniformLocations[name], vec4);

    private static int CreateAndAttachShader(ShaderType type, int handle, uint shaderFlags, uint shadingModel)
    {
        StringBuilder injectedCode = ValidateFlags(shaderFlags, shadingModel, type);
        StringBuilder sourse;

        if (type == ShaderType.VertexShader)
            sourse = new(Resources.UniversalVertexShader);
        else
            sourse = new(Resources.UniversalFragmentShader);

        sourse.Insert(21, injectedCode.ToString());
        
        var shaderDescriptor = GL.CreateShader(type);

#if DEBUG_MODE
        using (StreamWriter writer = new(new FileStream(Environment.CurrentDirectory + $"\\{type}.txt", FileMode.OpenOrCreate)))
        {
            writer.Write(sourse);
        }
#endif
        GL.ShaderSource(shaderDescriptor, sourse.ToString());
        CompileShader(shaderDescriptor);
        GL.AttachShader(handle, shaderDescriptor);

        return shaderDescriptor;
    }

    private static StringBuilder ValidateFlags(uint shaderFlags, uint shadingModel, ShaderType type)
    {
        var useLighting = (shaderFlags & 2) != 0;

        StringBuilder target = new();
        target.AppendLine(GLSLMacrosBuilder.GetBuildData(shaderFlags, shadingModel, useLighting));

        if (useLighting)
        {
            target.AppendLine(Encoding.UTF8.GetString(Resources.Structures));

            if ((shadingModel == 0 && type == ShaderType.FragmentShader) || (shadingModel == 1 && type == ShaderType.VertexShader))
                target.AppendLine(Encoding.UTF8.GetString(Resources.BlinnPhongFuncs));
            else if (shadingModel == 2 && type == ShaderType.FragmentShader)
                target.AppendLine(Encoding.UTF8.GetString(Resources.LambertianFuncs));
        }

        return target;
    }

    //-----ВРЕМЕННАЯ ПОМЕТКА-----//
    //BLINN_PHONG_SHADING_MODEL = 0,
    //GOURAND_SHADING_MODEL = 1,
    //lAMBERTIAN_SHADING_MODEL = 2,
    //OREN_NAYAR_SHADING_MODEL = 3,
    //DISNEY_BRDF_SHADING_MODEL = 4

    private static void CompileShader(int descriptor)
    {
        GL.CompileShader(descriptor);
        GL.GetShader(descriptor, ShaderParameter.CompileStatus, out int code);

        if (code == 0) SystemSettings.PrintMessage("Error. " + GL.GetShaderInfoLog(descriptor), Core.MessageTypes.ErrorMessage);
    }

    private Dictionary<string, int> GetUniforms()
    {
        GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out int uniformsCount);
        Dictionary<string, int> result = new(uniformsCount);

        for (int i = 0; i < uniformsCount; i++)
        {
            var key = GL.GetActiveUniform(Handle, i, out _, out _);
            var location = GL.GetUniformLocation(Handle, key);
            result.Add(key, location);
        }

        return result;
    }
}
