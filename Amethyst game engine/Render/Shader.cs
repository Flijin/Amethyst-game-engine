using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;
using System.Text;

namespace Amethyst_game_engine.Render;

internal class Shader : IDisposable
{
    private readonly Dictionary<string, int> _uniformLocations;

    public int Handle { get; private set; }

    public Shader(int shaderFlags)
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

    public void SetMatrix4(string name, float[,] matrix)
    {
        GL.UseProgram(Handle);

        var arr1D = new float[matrix.Length];
        var index = 0;

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                arr1D[index++] = matrix[i, j];
            }
        }

        GL.UniformMatrix4(_uniformLocations[name], 1, false, arr1D);
    }

    public unsafe void SetMatrix4(string name, float* matrixP)
    {
        GL.UseProgram(Handle);
        GL.UniformMatrix4(_uniformLocations[name], 1, false, matrixP);
    }

    public void SetVector3(string name, Vector3 vec)
    {
        GL.UseProgram(Handle);
        GL.Uniform3(_uniformLocations[name], vec);
    }

    public void SetFloat(string name, float value)
    {
        GL.UseProgram(Handle);
        GL.Uniform1(_uniformLocations[name], value);
    }

    public void SetInt(string name, int value)
    {
        GL.UseProgram(Handle);
        GL.Uniform1(_uniformLocations[name], value);
    }

    private static int CreateAndAttachShader(ShaderType type, int handle, int shaderFlags)
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

    private static StringBuilder ValidateFlags(int shaderFlags)
    {
        StringBuilder target = new();

        if ((shaderFlags & 0b_0001_00000000) != 0)
            target.AppendLine("#define USE_MESH_MATRIX");

        if ((shaderFlags & 0b_0001) != 0)
        {
            if ((shaderFlags & 0b_0010_00000000) != 0)
                target.AppendLine("#define USE_STL_COLORS");
            else
                target.AppendLine("#define USE_COLORS");
        }

        if ((shaderFlags & 0b_0010) != 0)
            target.AppendLine("#define USE_ALBEDO");

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
