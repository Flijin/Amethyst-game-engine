using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Game_Engine.Core.Render
{
    internal class Shader : IDisposable
    {
        private readonly Dictionary<string, int> _uniformLocations;

        public int Handle { get; private set; }

        public Shader(string vertexShaderPath, string fragmentShaderPath)
        {
            Handle = GL.CreateProgram();

            var vertexDescriptor = CreateAndAttachShader(vertexShaderPath, ShaderType.VertexShader, Handle);
            var fragmentDescriptor = CreateAndAttachShader(fragmentShaderPath, ShaderType.FragmentShader, Handle);

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
        public void SetMatrix4(string name, Matrix4 matrix) => GL.UniformMatrix4(_uniformLocations[name], true, ref matrix);

        public void SetMatrix4(string name, float[,] matrix)
        {
            if (matrix.GetLength(0) != 4 && matrix.GetLength(1) != 4)
                throw new ArgumentException("Размер матрицы должен быть 4x4");

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

        private static int CreateAndAttachShader(string path, ShaderType type, int handle)
        {
            var shaderSourse = File.ReadAllText(path);
            var shaderDescriptor = GL.CreateShader(type);

            GL.ShaderSource(shaderDescriptor, shaderSourse);
            CompileShader(shaderDescriptor);
            GL.AttachShader(handle, shaderDescriptor);

            return shaderDescriptor;
        }

        private static void CompileShader(int descriptor)
        {
            GL.CompileShader(descriptor);
            GL.GetShader(descriptor, ShaderParameter.CompileStatus, out int code);

            if (code == 0) PrintErrorMessage(GL.GetShaderInfoLog(descriptor));
        }

        private static void PrintErrorMessage(string message)
        {
            Program.ShowWindow(Program.WINDOW_DESCRIPTOR, Program.SW_SHOW);
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
}
