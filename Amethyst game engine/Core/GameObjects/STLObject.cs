﻿using Amethyst_game_engine.CameraModules;
using Amethyst_game_engine.Models.STLModule;
using OpenTK.Graphics.OpenGL4;

namespace Amethyst_game_engine.Core.GameObjects;

public class STLObject : GameObject
{
    public STLObject(STLModel model, bool useCamera) : base([model.mesh], useCamera, 0)
    {

    }

    internal override sealed void DrawObject(Camera? cam)
    {
        float[,] viewMatrix;
        float[,] projectionMatrix;
        var modelMatrix = ModelMatrix;

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
                _activeShader.SetMatrix4("model", modelMatrix);
                _activeShader.SetMatrix4("view", viewMatrix);
                _activeShader.SetMatrix4("projection", projectionMatrix);

                GL.DrawArrays((PrimitiveType)primitive.mode, 0, primitive.count);
            }
        }
    }
}
