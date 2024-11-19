using Amethyst_game_engine.CameraModules;
using Amethyst_game_engine.Models.STLModule;
using OpenTK.Graphics.OpenGL4;

namespace Amethyst_game_engine.Core;

public class STLObject : GameObject
{
    public STLObject(STLModel model, bool useCamera) : base([model.mesh], useCamera, 0)
    {

    }

    internal override void DrawObject(Camera? cam)
    {
        foreach (var mesh in _meshes)
        {
            foreach (var primitive in mesh.primitives)
            {
                GL.BindVertexArray(primitive.vao);

                _activeShader.Use();

                _activeShader.SetMatrix4("mesh", mesh.Matrix);
                _activeShader.SetMatrix4("model", _modelMatrix);
                _activeShader.SetMatrix4("view", cam?.ViewMatrix ?? Mathematics.UNIT_MATRIX);
                _activeShader.SetMatrix4("projection", cam?.ProjectionMatrix ?? Mathematics.UNIT_MATRIX);

                GL.DrawArrays((PrimitiveType)primitive.mode, 0, primitive.count);
            }
        }
    }
}
