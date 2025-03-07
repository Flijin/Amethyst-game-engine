using Amethyst_game_engine.Core;
using Amethyst_game_engine.Render;
using OpenTK.Graphics.ES30;
using System.Runtime.InteropServices;

namespace Amethyst_game_engine.Models.STLModule;

public readonly struct STLModel
{
    internal const int VALID_FLAGS = 0x_3C03;

    internal readonly int renderKeys = (int)RenderSettings.All;
    internal readonly Mesh mesh;

    public string Header { get; }
    public int TrianglesCount { get; }

    public STLModel(string path, Material material, RenderSettings settings) : this(path, settings)
    {
        mesh.primitives[0].material = material;
    }

    public STLModel(string path) : this(path, RenderSettings.All) { }

    public unsafe STLModel(string path, RenderSettings settings)
    {
        using BinaryReader reader = new(File.OpenRead(path));

        Header = new string(reader.ReadChars(80)).Trim('\0');
        TrianglesCount = reader.ReadInt32();

        var vertexIndex = 0;

        var vertexArrayObject = GL.GenVertexArray();
        var bytesCount = TrianglesCount * 36;
        var bufferLenght = bytesCount;
        var usedBuffers = 1;

        Span<float> vertices;
        Span<float> colors;
        Span<float> normals;

        if (((int)settings & 0b_0001) != 0)
        {
            renderKeys |= 0b_0001;
            bytesCount += bufferLenght;
            usedBuffers += 1;
        }

        if (((int)settings & 0b_0010) != 0)
        {
            renderKeys |= 0b_0010;
            bytesCount += bufferLenght;
            usedBuffers += 1;
        }

        int[] bufferHandles = new int[usedBuffers];

        GL.BindVertexArray(vertexArrayObject);

        var modelPrimitive = new Primitive(vertexArrayObject);
        var attributesCount = TrianglesCount * 9;

#pragma warning disable CS9081

        //-----------------------------------
        // Stack fill limit 800 KB
        // You can change it at your own risk
        //-----------------------------------

        if (bytesCount > 819200)
        {
            vertices = new(new float[attributesCount]);
            colors = (renderKeys & 0b_0001) != 0 ? new(new float[attributesCount]) : null;
            normals = (renderKeys & 0b_0010) != 0 ? new(new float[attributesCount]) : null;
        }
        else
        {
            vertices = stackalloc float[attributesCount];
            colors = (renderKeys & 0b_0001) != 0 ? stackalloc float[attributesCount] : null;
            normals = (renderKeys & 0b_0010) != 0 ? stackalloc float[attributesCount] : null;
        }

#pragma warning restore

        modelPrimitive.count = TrianglesCount * 3;

        for (int i = 0; i < TrianglesCount; i++)
        {
            if ((renderKeys & 0b_0010) != 0)
            {
                for (int j = 0; j < 3; j++)
                {
                    normals[i * 9 + j] = normals[i * 9 + j + 3] = normals[i * 9 + j + 6] = reader.ReadSingle();
                }
            }
            else
            {
                reader.BaseStream.Position += 12;
            }

            for (int j = 0; j < 9; j++)
            {
                vertices[vertexIndex++] = reader.ReadSingle();
            }

            if ((renderKeys & 0b_0001) != 0)
            {
                ushort attributeByteCount = reader.ReadUInt16();

                float r = 0f;
                float g = 0f;
                float b = 0f;

                if (attributeByteCount >> 15 != 0)
                {
                    r = ((attributeByteCount & 0b_01111100_00000000) >> 10) / 31f;
                    g = ((attributeByteCount & 0b_00000011_11100000) >> 5) / 31f;
                    b = (attributeByteCount & 0b_00000000_00011111) / 31f;
                }

                colors[i * 9] = colors[i * 9 + 3] = colors[i * 9 + 6] = r;
                colors[i * 9 + 1] = colors[i * 9 + 4] = colors[i * 9 + 7] = g;
                colors[i * 9 + 2] = colors[i * 9 + 5] = colors[i * 9 + 8] = b;
            }
            else
            {
                reader.BaseStream.Position += 2;
            }
        }

        void AddAttribute(int location, float* ptr)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferHandles[location] = GL.GenBuffer());
            GL.BufferData(BufferTarget.ArrayBuffer, attributesCount * sizeof(float), (nint)ptr, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(location, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(location);
        }

        fixed (float* ptr = &MemoryMarshal.GetReference(vertices))
            AddAttribute(0, ptr);

        if (colors != null)
        {
            fixed (float* ptr = &MemoryMarshal.GetReference(colors))
                AddAttribute(1, ptr);
        }

        if (normals != null)
        {
            fixed (float* ptr = &MemoryMarshal.GetReference(normals))
                AddAttribute(2, ptr);
        }

        unsafe
        {
            mesh = new([modelPrimitive], bufferHandles) { Matrix = null };
        }
    }
}
