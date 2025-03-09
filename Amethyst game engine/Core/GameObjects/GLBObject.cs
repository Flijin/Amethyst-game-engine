using Amethyst_game_engine.CameraModules;
using Amethyst_game_engine.Models.GLBModule;
using OpenTK.Graphics.ES30;
using System.Diagnostics.CodeAnalysis;

namespace Amethyst_game_engine.Core.GameObjects;

/*
 * Ooops, There are still bugs here
 */

[Obsolete("I've done some shit here, Dont use it")]
public class GLBObject : GameObject
{
    public GLBObject(GLBModel model, bool useCamera) : base(null, useCamera)
    {
        
    }

    internal override unsafe void DrawObject(Camera? cam)
    {
        //float* viewMatrix;
        //float* projectionMatrix;

        //if (cam is null || _useCamera == false)
        //{
        //    viewMatrix = Mathematics.IDENTITY_MATRIX;
        //    projectionMatrix = Mathematics.IDENTITY_MATRIX;
        //}
        //else
        //{
        //    viewMatrix = cam.ViewMatrix;
        //    projectionMatrix = cam.ProjectionMatrix;
        //}

        //foreach (var mesh in _meshes)
        //{
        //    foreach (var primitive in mesh.primitives)
        //    {
        //        GL.BindVertexArray(primitive.vao);
        //        //GL.BindTexture(TextureTarget.Texture2D, primitive.material[Models.MaterialsProperties.Albedo].texture);

        //        _activeShader.Use();

        //        _activeShader.SetInt("albeloTextute", 0);
        //        _activeShader.SetMatrix4("mesh", mesh.Matrix);
        //        _activeShader.SetMatrix4("model", ModelMatrix);
        //        _activeShader.SetMatrix4("view", viewMatrix);
        //        _activeShader.SetMatrix4("projection", projectionMatrix);

        //        if (primitive.isIndexedGeometry)
        //            GL.DrawElements((PrimitiveType)primitive.mode, primitive.count, (DrawElementsType)primitive.componentType, 0);
        //        else
        //            GL.DrawArrays((PrimitiveType)primitive.mode, 0, primitive.count);
        //    }
        //}
    }
}
