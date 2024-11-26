﻿using Amethyst_game_engine.CameraModules;
using Amethyst_game_engine.Models.GLBModule;
using OpenTK.Graphics.OpenGL4;

namespace Amethyst_game_engine.Core.GameObjects;

public class GLBObject : GameObject
{
    public GLBObject(GLBModel model, bool useCamera) : base(model.meshes, useCamera, 64, 256)
    {
        
    }

    internal override unsafe void DrawObject(Camera? cam)
    {
        float* viewMatrix;
        float* projectionMatrix;

        if (cam is null || _useCamera == false)
        {
            viewMatrix = Mathematics.IDENTITY_MATRIX;
            projectionMatrix = Mathematics.IDENTITY_MATRIX;
        }
        else
        {
            viewMatrix = cam.ViewMatrix;
            projectionMatrix = cam.ProjectionMatrix;
        }


        foreach (var mesh in _meshes)
        {
            foreach (var primitive in mesh.primitives)
            {
                GL.BindVertexArray(primitive.vao);
                _activeShader.Use();

                _activeShader.SetMatrix4("mesh", mesh.Matrix);
                _activeShader.SetMatrix4("model", ModelMatrix);
                _activeShader.SetMatrix4("view", viewMatrix);
                _activeShader.SetMatrix4("projection", projectionMatrix);

                if (primitive.isIndexedGeometry)
                    GL.DrawElements((PrimitiveType)primitive.mode, primitive.count, (DrawElementsType)primitive.componentType, 0);
                else
                    GL.DrawArrays((PrimitiveType)primitive.mode, 0, primitive.count);
            }
        }
    }
}
