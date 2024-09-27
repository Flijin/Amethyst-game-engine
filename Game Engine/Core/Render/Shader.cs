﻿using OpenTK.Graphics.OpenGL4;

namespace Game_Engine.Core.Render
{
    internal class Shader : IDisposable
    {
        public int Handle { get; private set; }

        public Shader(string vertexShaderPath, string fragmentShaderPath)
        {
            Handle = GL.CreateProgram();

            var vertexDescriptor = CreateAndAttachShader(vertexShaderPath, ShaderType.VertexShader);
            var fragmentDescriptor = CreateAndAttachShader(fragmentShaderPath, ShaderType.FragmentShader);

            GL.LinkProgram(Handle);
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int code);

            if (code == 0) PrintErrorMessage(GL.GetProgramInfoLog(Handle));

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
                GL.AttachShader(Handle, shaderDescriptor);

                return shaderDescriptor;
            }

            void ClearShader(int descriptor)
            {
                GL.DetachShader(Handle, descriptor);
                GL.DeleteShader(descriptor);
            }
        }

        public void Use() => GL.UseProgram(Handle);
        public void Dispose() => GL.DeleteProgram(Handle);
    }
}
