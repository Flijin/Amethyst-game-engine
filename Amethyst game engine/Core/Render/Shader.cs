#define DEBUG_MODE

using Amethyst_game_engine.Core.Render.Components;
using Amethyst_game_engine.Core.Utilities;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Amethyst_game_engine.Core.Render;

internal sealed class Shader : IDisposable
{
    private const int START_WRITE = 16;

    private readonly Dictionary<string, int> _uniformLocations;

    public ShaderBuildingProps Options { get; }

    public int Handle { get; private set; }

    public Shader(ShaderBuildingProps props)
    {
        Options = props;
        Handle = GL.CreateProgram();

        var vertexDescriptor = CreateAndAttachShader(ShaderType.VertexShader, Handle, props);
        var fragmentDescriptor = CreateAndAttachShader(ShaderType.FragmentShader, Handle, props);

        GL.LinkProgram(Handle);
        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int code);

        if (code == 0) System.PrintMessage(GL.GetShaderInfoLog(Handle), MessageTypes.ErrorMessage);

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

    private static int CreateAndAttachShader(ShaderType type, int handle, ShaderBuildingProps props)
    {
        StringBuilder injectedCode = ValidateFlags(props, type);
        StringBuilder sourse;

        if (type == ShaderType.VertexShader)
            sourse = new(Resources.UniversalVertexShader);
        else
            sourse = new(Resources.UniversalFragmentShader);

        sourse.Insert(START_WRITE, injectedCode.ToString());
        
        var shaderDescriptor = GL.CreateShader(type);

#if DEBUG_MODE
        using (StreamWriter writer = new(new FileStream(Environment.CurrentDirectory + $"\\{type}.txt", FileMode.Create)))
        {
            writer.Write(sourse);
        }
#endif
        GL.ShaderSource(shaderDescriptor, sourse.ToString());
        CompileShader(shaderDescriptor);
        GL.AttachShader(handle, shaderDescriptor);

        return shaderDescriptor;
    }

    private static StringBuilder ValidateFlags(ShaderBuildingProps props, ShaderType type)
    {
        StringBuilder target = new();
        GLSLMacrosBuilder.BuildMacrosByFlags(props, target);

        return target;
    }

    private static void CompileShader(int descriptor)
    {
        GL.CompileShader(descriptor);
        GL.GetShader(descriptor, ShaderParameter.CompileStatus, out int code);

        if (code == 0) System.PrintMessage(GL.GetShaderInfoLog(descriptor), MessageTypes.ErrorMessage);
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
