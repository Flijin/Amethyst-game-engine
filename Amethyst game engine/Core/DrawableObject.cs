using Amethyst_game_engine.CameraModules;
using Amethyst_game_engine.Core.Render;
using OpenTK.Graphics.OpenGL4;

namespace Amethyst_game_engine.Core;

public abstract class DrawableObject : IDisposable
{
    private protected float[,] _modelMatrix;
    private protected readonly float[] _vertices;
    private protected readonly BufferUsageHint _usageHint = BufferUsageHint.StaticDraw;

    public int VAO { get; set; }
    public int VBO { get; set; }

    protected DrawableObject(float[] vertices)
    {
        _vertices = vertices;

        _modelMatrix =
            new float[4, 4]
            {
                { 1, 0, 0, 0 },
                { 0, 1, 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 },
            };

        LoadObjectInGPU();
    }

    private protected void UploadObjectFromGPU() => GL.DeleteBuffer(VBO);
    internal abstract void DrawObject(RenderCore core, Camera cam);

    private protected void LoadObjectInGPU()
    {
        var vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float),
                                                _vertices, _usageHint);

        var vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(vertexArrayObject);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        VAO = vertexArrayObject;
        VBO = vertexBufferObject;
    }

    public void Dispose()
    {
        UploadObjectFromGPU();
    }
}
