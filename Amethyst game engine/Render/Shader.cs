using Amethyst_game_engine.Models;
using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;
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

        if (code == 0) PrintErrorMessage(GL.GetProgramInfoLog(Handle));

        ClearShader(vertexDescriptor);
        ClearShader(fragmentDescriptor);

        _uniformLocations = GetUniforms();

        void ClearShader(int descriptor)
        {
            GL.DetachShader(Handle, descriptor);
            GL.DeleteShader(descriptor);
        }
    }

    public void Use() => GL.UseProgram(Handle);
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

    public void SetVectors3(Dictionary<string, Vector3> data)
    {
        var pairs = data.ToArray();

        for (int i = 0; i < pairs.Length; i++)
        {
            GL.Uniform3(_uniformLocations[pairs[i].Key], pairs[i].Value);
        }
    }

    public unsafe void SetMatrix4(string name, float* matrixPtr)
    {
        GL.UniformMatrix4(_uniformLocations[name], 1, false, matrixPtr);
    }

    public void SetFloat(string name, float value)
    {
        GL.Uniform1(_uniformLocations[name], value);
    }

    public void SetInt(string name, int value)
    {
        GL.Uniform1(_uniformLocations[name], value);
    }

    public void SetVector3(string name, Vector3 vec)
    {
        GL.Uniform3(_uniformLocations[name], vec);
    }

    private static int CreateAndAttachShader(ShaderType type, int handle, uint shaderFlags)
    {
        StringBuilder defines = ValidateFlags(shaderFlags);
        StringBuilder sourse;

        if (type == ShaderType.VertexShader)
            sourse = new(Resources.UniversalVertexShader);
        else
            sourse = new(Resources.UniversalFragmentShader);

        sourse.Insert(19, defines.ToString());
        
        var shaderDescriptor = GL.CreateShader(type);

        GL.ShaderSource(shaderDescriptor, sourse.ToString());
        CompileShader(shaderDescriptor);
        GL.AttachShader(handle, shaderDescriptor);

        return shaderDescriptor;
    }

    private static StringBuilder ValidateFlags(uint shaderFlags)
    {
        StringBuilder target = new();

        var renderSettings = (RenderSettings)(shaderFlags & 0b_00000000_11111111_11111111_11111111);
        var modelSettings = (ModelSettings)(shaderFlags & 0b_11111111_00000000_00000000_00000000);

        target.AppendLine(renderSettings.ToMacrosString());
        target.AppendLine(modelSettings.ToMacrosString());

        return target;
    }

    private static void CompileShader(int descriptor)
    {
        GL.CompileShader(descriptor);
        GL.GetShader(descriptor, ShaderParameter.CompileStatus, out int code);

        if (code == 0) PrintErrorMessage(GL.GetShaderInfoLog(descriptor));
    }

    private static void PrintErrorMessage(string message)
    {
        SystemSettings.ShowWindow(SystemSettings.SW_SHOW);
        Console.Write(message);
        Console.ReadKey();
        Environment.Exit(0);
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
