using Amethyst_game_engine.Core;
using Amethyst_game_engine.Render;
using OpenTK.Graphics.ES30;
using System.Runtime.InteropServices;

namespace Amethyst_game_engine.Models.STLModule;

public readonly struct STLModel : IModel
{
    internal const uint MODEL_SPECIFICITY = (uint)ModelSettings.USE_COLOR_5_BITS;
    internal readonly Mesh mesh;

    public string Header { get; }
    public int TrianglesCount { get; }

    public STLModel(string path) : this(path, RenderSettings.All, new Material(0)) { }

    public STLModel(string path, RenderSettings settings) : this(path, settings, new Material(0)) { }

    public STLModel(string path, Material material) : this(path, RenderSettings.All, material) { }

    public unsafe STLModel(string path, RenderSettings settings, Material material)
    {
        int settings_int = (int)settings;
        material.materialKey |= MODEL_SPECIFICITY;

        using BinaryReader reader = new(File.OpenRead(path));

        Header = new string(reader.ReadChars(80)).Trim('\0');
        TrianglesCount = reader.ReadInt32();

        var vertexIndex = 0;

        var vertexArrayObject = GL.GenVertexArray();
        var bytesCount = TrianglesCount * 36;
        var bufferLenght = bytesCount;
        var usedBuffers = 1;

        scoped Span<float> vertices;
        scoped Span<byte> colors;
        scoped Span<float> normals;

        if ((settings_int & 0b_0001) != 0)
        {
            material.materialKey |= 0b_0001;
            bytesCount += bufferLenght;
            usedBuffers += 1;
        }

        if ((settings_int & 0b_0010) != 0)
        {
            material.materialKey |= 0b_0010;
            bytesCount += bufferLenght;
            usedBuffers += 1;
        }

        int[] bufferHandles = new int[usedBuffers];

        GL.BindVertexArray(vertexArrayObject);

        var modelPrimitive = new Primitive(vertexArrayObject) { Material = material };
        var attributesCount = TrianglesCount * 9;

        //-----------------------------------
        // Stack fill limit 800 KB
        // You can change it at your own risk
        //-----------------------------------

        if (bytesCount > 819200)
        {
            vertices = new(new float[attributesCount]);
            colors = (settings_int & 0b_0001) != 0 ? new(new byte[attributesCount]) : null;
            normals = (settings_int & 0b_0010) != 0 ? new(new float[attributesCount]) : null;
        }
        else
        {
            vertices = stackalloc float[attributesCount];
            colors = (settings_int & 0b_0001) != 0 ? stackalloc byte[attributesCount] : null;
            normals = (settings_int & 0b_0010) != 0 ? stackalloc float[attributesCount] : null;
        }

        modelPrimitive.count = TrianglesCount * 3;

        for (int i = 0; i < TrianglesCount; i++)
        {
            if ((settings_int & 0b_0010) != 0)
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

            if ((settings_int & 0b_0001) != 0)
            {
                ushort attributeByteCount = reader.ReadUInt16();

                byte r = default;
                byte g = default;
                byte b = default;

                if (attributeByteCount >> 15 != 0)
                {
                    r = (byte)((attributeByteCount & 0b_01111100_00000000) >> 10);
                    g = (byte)((attributeByteCount & 0b_00000011_11100000) >> 5);
                    b = (byte)(attributeByteCount & 0b_00000000_00011111);
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

        void AddAttribute(int location, void* ptr, Type typeOfData)
        {
            var typeOfAtribute = Marshal.SizeOf(typeOfData) == 4 ? VertexAttribPointerType.Float : VertexAttribPointerType.UnsignedByte;

            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferHandles[location] = GL.GenBuffer());
            GL.BufferData(BufferTarget.ArrayBuffer, attributesCount * Marshal.SizeOf(typeOfData), (nint)ptr, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(location, 3, typeOfAtribute, false, 3 * Marshal.SizeOf(typeOfData), 0);
            GL.EnableVertexAttribArray(location);
        }

        fixed (float* ptr = &MemoryMarshal.GetReference(vertices))
            AddAttribute(0, ptr, typeof(float));

        if (colors != null)
        {
            fixed (byte* ptr = &MemoryMarshal.GetReference(colors))
                AddAttribute(1, ptr, typeof(byte));
        }

        if (normals != null)
        {
            fixed (float* ptr = &MemoryMarshal.GetReference(normals))
                AddAttribute(2, ptr, typeof(float));
        }

        unsafe
        {
            mesh = new([modelPrimitive], bufferHandles) { Matrix = null };
        }
    }

    Mesh[] IModel.GetMeshes() => [mesh];

    void IModel.RebuildShaders(uint renderKeys) => mesh.RebuildShaders(renderKeys, MODEL_SPECIFICITY);

    uint IModel.GetModelSettings() => MODEL_SPECIFICITY;

    public void Dispose()
    {
        mesh.Dispose();
    }
}
