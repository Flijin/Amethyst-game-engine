using OpenTK.Graphics.OpenGL4;

namespace Game_Engine
{
    internal class Shader : IDisposable
    {
        private readonly int _handle;
        private bool _hasDisposed;

        public Shader(string vertexShaderPath, string fragmentShaderPath)
        {
            _handle = GL.CreateProgram();

            var vertexDescriptor = CreateAndAttachShader(vertexShaderPath, ShaderType.VertexShader);
            var fragmentDescriptor = CreateAndAttachShader(fragmentShaderPath, ShaderType.FragmentShader);

            GL.LinkProgram(_handle);
            GL.GetProgram(_handle, GetProgramParameterName.LinkStatus, out int code);

            if (code == 0) PrintErrorMessage(GL.GetProgramInfoLog(_handle));

            ClearShader(vertexDescriptor);
            ClearShader(fragmentDescriptor);

            static void CompileShader(int descriptor)
            {
                GL.CompileShader(descriptor);
                GL.GetShader(descriptor, ShaderParameter.CompileStatus, out int code);

                if (code == 0) PrintErrorMessage(GL.GetShaderInfoLog(descriptor));
            }

            static void PrintErrorMessage(string message)
            {
                Program.ShowWindow(Program.WINDOW_DESCRIPTOR, Program.SW_SHOW);
                Console.Write(message);
                Console.ReadKey();
                Environment.Exit(0);
            }

            int CreateAndAttachShader(string path, ShaderType type)
            {
                var shaderSourse = File.ReadAllText(path);
                var shaderDescriptor = GL.CreateShader(type);

                GL.ShaderSource(shaderDescriptor, shaderSourse);
                CompileShader(shaderDescriptor);
                GL.AttachShader(_handle, shaderDescriptor);

                return shaderDescriptor;
            }

            void ClearShader(int descriptor)
            {
                GL.DetachShader(_handle, descriptor);
                GL.DeleteShader(descriptor);
            }
        }

        ~Shader()
        {
            if (_hasDisposed == false)
            {
                Program.ShowWindow(Program.WINDOW_DESCRIPTOR, Program.SW_SHOW);
                Console.Write("Не был использован метод Dispose утечка памяти GPU");
            }  
        }

        public void Use() => GL.UseProgram(_handle);

        public void Dispose()
        {
            if (_hasDisposed == false)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        public virtual void Dispose(bool disposing)
        {
            if (_hasDisposed == false)
            {
                GL.DeleteProgram(_handle);
                _hasDisposed = true;
            }
        }
    }
}
