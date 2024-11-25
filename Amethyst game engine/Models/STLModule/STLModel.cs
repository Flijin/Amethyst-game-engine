using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Amethyst_game_engine.Models.STLModule;

public readonly struct STLModel
{
    internal readonly float[] normals;
    internal readonly Mesh mesh;

    public string Header { get; }
    public uint TrianglesCount { get; }
    public Vector3i DefaultColor { readonly get; init; } = new Vector3i(16, 16, 16);

    public unsafe STLModel(string path)
    {
        using BinaryReader br = new(File.OpenRead(path));

        Header = new string(br.ReadChars(80)).Trim('\0');
        TrianglesCount = br.ReadUInt32();

        var vertexIndex = 0;
        var normalIndex = 0;

        int[] bufferHandles = new int[2];
        var vertexArrayObject = GL.GenVertexArray();

        GL.BindVertexArray(vertexArrayObject);

        var modelPrimitive = new Primitive(vertexArrayObject);

        Span<float> vertices;
        Span<byte> colors;

        var attributesCount = (int)TrianglesCount * 9;

#pragma warning disable CS9081
        if (TrianglesCount > 20000)
        {
            vertices = new(new float[attributesCount]);
            colors = new(new byte[attributesCount]);
        }
        else
        {
            vertices = stackalloc float[attributesCount];
            colors = stackalloc byte[attributesCount];
        }
#pragma warning restore

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

            byte r = (byte)DefaultColor.X;
            byte g = (byte)DefaultColor.Y;
            byte b = (byte)DefaultColor.Z;

            if (attributeByteCount >> 15 != 0)
            {
                r = (byte)((attributeByteCount & 0b_01111100_00000000) >> 10);
                g = (byte)((attributeByteCount & 0b_00000011_11100000) >> 5);
                b = (byte) (attributeByteCount & 0b_00000000_00011111);
            }

            colors[i * 9] = colors[i * 9 + 3] = colors[i * 9 + 6] = r;
            colors[i * 9 + 1] = colors[i * 9 + 4] = colors[i * 9 + 7] = g;
            colors[i * 9 + 2] = colors[i * 9 + 5] = colors[i * 9 + 8] = b;
        }

        fixed (float* ptr = &MemoryMarshal.GetReference(vertices))
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferHandles[0] = GL.GenBuffer());
            GL.BufferData(BufferTarget.ArrayBuffer, attributesCount * sizeof(float), (nint)ptr, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
        }

        fixed (byte* ptr = &MemoryMarshal.GetReference(colors))
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferHandles[0] = GL.GenBuffer());
            GL.BufferData(BufferTarget.ArrayBuffer, attributesCount * sizeof(byte), (nint)ptr, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Byte, false, 3 * sizeof(byte), 0);
            GL.EnableVertexAttribArray(1);
        }

        unsafe
        {
            mesh = new([modelPrimitive], bufferHandles) { Matrix = null };
        }
    }
}
