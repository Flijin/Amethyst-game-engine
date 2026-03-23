using Amethyst_game_engine.Core.GameObjects.Components;
using Amethyst_game_engine.Models.STLModule;
using OpenTK.Graphics.OpenGL4;

namespace Amethyst_game_engine.Core.GameObjects;

public class STLGameObject : DrawableObject
{
    public STLGameObject(STLModel model)
    {
        BuildObject(model);
    }

    private void BuildObject(STLModel model)
    {
        var stlPrimitive = model.MeshData.Primitives[0];
        
        var vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);

        Primitive primitive = new(vao, new Primitive.Options()
        {
            Count = stlPrimitive.Options.Count * 3,
            IsIndexedGeometry = false,
            Mode = PrimitiveType.Triangles
        })
        {
            SettingsFromModel = model.Settings
        };

        var vertexBuffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);

        GL.BufferData(BufferTarget.ArrayBuffer,
                      stlPrimitive.Vertices.Length,
                      stlPrimitive.Vertices,
                      BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        primitive.GLBuffers.Add(vertexBuffer);

        if (stlPrimitive.Normals is not null)
        {
            var normalBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, normalBuffer);

            GL.BufferData(BufferTarget.ArrayBuffer,
                          stlPrimitive.Normals.Length,
                          stlPrimitive.Normals,
                          BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);

            primitive.GLBuffers.Add(normalBuffer);
        }

        if (stlPrimitive.Colors is not null)
        {
            var colorBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, colorBuffer);

            GL.BufferData(BufferTarget.ArrayBuffer,
                          stlPrimitive.Colors.Length,
                          stlPrimitive.Colors,
                          BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(2);

            primitive.GLBuffers.Add(colorBuffer);

            GL.BindVertexArray(0);
        }

        Meshes = [new Mesh([primitive], false)];
    }
}
