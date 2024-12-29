using Game_Engine.Core.CameraModules;
using Game_Engine.Enums;
using OpenTK.Graphics.OpenGL4;

namespace Game_Engine.Core.Render;

internal class RenderCore : IDisposable
{
    private readonly Shader _shader = new(@"C:\Users\it_ge\source\repos\Game Engine\Game Engine\Shaders\Shader.vert",
                                          @"C:\Users\it_ge\source\repos\Game Engine\Game Engine\Shaders\Shader.frag");

    public void Dispose() => _shader.Dispose();

    public void DrawSTLMolel(StaticGameObject3D obj, Camera cam)
    {
        GL.BindVertexArray(obj.VAO);
        _shader.Use();

        _shader.SetMatrix4("model", obj.ModelMatrix);
        _shader.SetMatrix4("view", cam.ViewMatrix);
        _shader.SetMatrix4("projection", cam.ProjectionMatrix);

        var index = 0;

        for (int i = 0; i < obj.Model.TrianglesCount; i++)
        {
            _shader.SetVector3("aColor", obj.Model.GetData(AttribTypes.Color, i));
            GL.DrawArrays(PrimitiveType.Triangles, index, 3);
            index += 3;
        }
    }
}
