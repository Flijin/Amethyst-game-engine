using Amethyst_game_engine.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Models.STLModule;

public readonly struct STLModel
{
    internal readonly float[] normals;
    internal readonly Mesh mesh;

    public string Header { get; }
    public uint TrianglesCount { get; }
    public Vector3 DefaultColor { get; init; } = new Vector3(0.5f, 0.5f, 0.5f);

    public STLModel(string path)
    {
        using BinaryReader br = new(File.OpenRead(path));

        Header = new string(br.ReadChars(80)).Trim('\0');
        TrianglesCount = br.ReadUInt32();

        var vertexIndex = 0;
        var normalIndex = 0;

        var bufferHandles = new int[2];
        var vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(vertexArrayObject);

        var modelPrimitive = new Primitive(vertexArrayObject);

        var vertices = new float[TrianglesCount * 9];

        var colors = new float[vertices.Length];
        normals = new float[TrianglesCount * 3];

        modelPrimitive.count = normals.Length;

        for (int i = 0; i < TrianglesCount; i++)
        {
            for (int vectors = 0; vectors < 3; vectors++)
            {
                normals[normalIndex++] = br.ReadSingle();
            }

            for (int vectors = 0; vectors < 9; vectors++)
            {
                vertices[vertexIndex++] = br.ReadSingle();
            }

            ushort attributeByteCount = br.ReadUInt16();

            var r = DefaultColor.X;
            var g = DefaultColor.Y;
            var b = DefaultColor.Z;

            if (attributeByteCount >> 15 != 0)
            {
                r = (attributeByteCount & 0b_01111100_00000000) / 32768f;
                g = (attributeByteCount & 0b_00000011_11100000) / 1024f;
                b = (attributeByteCount & 0b_00000000_00011111) / 32f;
            }

            for (int j = 0; j < 3; j++)
            {
                var offset = i * 9 + 3 * j;

                colors[offset] = r;
                colors[offset + 1] = g;
                colors[offset + 2] = b;
            }
        }

        AddAttribute(vertices, 0);
        AddAttribute(colors, 1);

        void AddAttribute(float[] buffer, int location)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferHandles[location] = GL.GenBuffer());
            GL.BufferData(BufferTarget.ArrayBuffer, buffer.Length * sizeof(float), buffer, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(location, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(location);
        }

        mesh = new([modelPrimitive], bufferHandles) { Matrix = Mathematics.IDENTITY_MATRIX };
    }
}
