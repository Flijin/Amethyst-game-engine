﻿using Amethyst_game_engine.CameraModules;
using Amethyst_game_engine.Core;
using Amethyst_game_engine.Models.GLBModule;
using Amethyst_game_engine.Models.STLModule;
using OpenTK.Graphics.OpenGL4;

namespace Amethyst_game_engine.Render;

internal class RenderCore : IDisposable
{
    private readonly Shader _shader = new();

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

    public void DrawObject(Mesh mesh, Camera cam)
    {
        foreach (var primitive in mesh.primitives)
        {
            GL.BindVertexArray(primitive.vao);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, primitive.textureHandle);

            _shader.Use();

            _shader.SetMatrix4("model", mesh.matrix);
            _shader.SetMatrix4("view", cam.ViewMatrix);
            _shader.SetMatrix4("projection", cam.ProjectionMatrix);
            //_shader.SetVector3("aColor", new OpenTK.Mathematics.Vector3(0.5f));

            if (primitive.isIndexedGeometry)
                GL.DrawElements((PrimitiveType)primitive.mode, primitive.count, (DrawElementsType)primitive.componentType, 0);
            else
                GL.DrawArrays((PrimitiveType)primitive.mode, 0, primitive.count);
        }
    }
}
