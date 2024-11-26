using Amethyst_game_engine.Render;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Amethyst_game_engine.Models.STLModule;

//USE_COLOR = 1, //Атрибуты
//UseAlbedoMap = 2, //карта
//UseMetallicRoughness = 4, //Коэф, карта
//UseNormalMap = 8, //карта
//UseNormals = 16, //Атрибуты
//Occlusion = 32, //Коэф, карта
//Emissive = 64, //Коэф, карта

//USE_MESH_MATRIX = 256
//USE_COLOR_5_BITS = 512

public readonly struct STLModel
{
    internal readonly float[] normals;
    internal readonly Mesh mesh;

    internal readonly int _renderProfile = 0b_00010001;

    public string Header { get; }
    public int TrianglesCount { get; }
    public Vector3i DefaultColor { readonly get; init; } = new Vector3i(16, 16, 16);

    public unsafe STLModel(string path, RenderSettings settings)
    {
        _renderProfile &= (int)settings;
        using BinaryReader br = new(File.OpenRead(path));

        Header = new string(br.ReadChars(80)).Trim('\0');
        TrianglesCount = br.ReadInt32();

        var vertexIndex = 0;
        var normalIndex = 0;

        var vertexArrayObject = GL.GenVertexArray();
        var bytesCount = TrianglesCount * 36;
        var usedBuffers = 1;

        if ((_renderProfile & 0b_0001) != 0)
        {
            bytesCount += TrianglesCount * 9;
            usedBuffers += 1;
        }

        int[] bufferHandles = new int[usedBuffers];

        GL.BindVertexArray(vertexArrayObject);

        var modelPrimitive = new Primitive(vertexArrayObject);

        Span<float> vertices;
        Span<byte> colors;

        var attributesCount = TrianglesCount * 9;

#pragma warning disable CS9081
        if (bytesCount > 800000)
        {
            vertices = new(new float[attributesCount]);
            colors = (_renderProfile & 0b_0001) != 0 ? new(new byte[attributesCount]) : null;
        }
        else
        {
            vertices = stackalloc float[attributesCount];
            colors = (_renderProfile & 0b_0001) != 0 ? stackalloc byte[attributesCount] : null;
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

            if ((_renderProfile & 0b_0001) != 0)
            {
                ushort attributeByteCount = br.ReadUInt16();

                byte r = (byte)DefaultColor.X;
                byte g = (byte)DefaultColor.Y;
                byte b = (byte)DefaultColor.Z;

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
                br.BaseStream.Position += 2;
        }

        fixed (float* ptr = &MemoryMarshal.GetReference(vertices))
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferHandles[0] = GL.GenBuffer());
            GL.BufferData(BufferTarget.ArrayBuffer, attributesCount * sizeof(float), (nint)ptr, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
        }
        
        if (colors != null)
        {
            fixed (byte* ptr = &MemoryMarshal.GetReference(colors))
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, bufferHandles[1] = GL.GenBuffer());
                GL.BufferData(BufferTarget.ArrayBuffer, attributesCount * sizeof(byte), (nint)ptr, BufferUsageHint.DynamicDraw);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Byte, false, 3 * sizeof(byte), 0);
                GL.EnableVertexAttribArray(1);
            }
        }

        unsafe
        {
            mesh = new([modelPrimitive], bufferHandles) { Matrix = null };
        }
    }
}
