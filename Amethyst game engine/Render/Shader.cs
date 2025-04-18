﻿using Amethyst_game_engine.Core.Light;
using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Amethyst_game_engine.Render;

internal class Shader : IDisposable
{
    private readonly Dictionary<string, int> _uniformLocations;

    public int Handle { get; private set; }

    public Shader(uint shaderFlags)
    {
        Handle = GL.CreateProgram();

        var vertexDescriptor = CreateAndAttachShader(ShaderType.VertexShader, Handle, shaderFlags);
        var fragmentDescriptor = CreateAndAttachShader(ShaderType.FragmentShader, Handle, shaderFlags);

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

    private static int CreateAndAttachShader(ShaderType type, int handle, uint shaderFlags)
    {
        StringBuilder defines = ValidateFlags(shaderFlags);
        StringBuilder sourse;

        if (type == ShaderType.VertexShader)
            sourse = new(Resources.UniversalVertexShader);
        else
            sourse = new(Resources.UniversalFragmentShader);

        sourse.Insert(21, defines.ToString());
        
        var shaderDescriptor = GL.CreateShader(type);

        GL.ShaderSource(shaderDescriptor, sourse.ToString());
        CompileShader(shaderDescriptor);
        GL.AttachShader(handle, shaderDescriptor);

        return shaderDescriptor;
    }

    private static StringBuilder ValidateFlags(uint shaderFlags)
    {
        StringBuilder target = new();

        target.AppendLine(shaderFlags.ToMacrosString());

        if ((shaderFlags & 2) != 0)
            target.AppendLine(LightManager.GetDefines());

        return target;
    }

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
