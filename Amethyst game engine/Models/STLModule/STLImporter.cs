using System.Text;
using Amethyst_game_engine.Core.GameObjects.Components;
using Amethyst_game_engine.Core.Render.Settings;
using Amethyst_game_engine.Core.Utilities;
using Amethyst_game_engine.Models.Components;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Models.STLModule;

public static class STLImporter
{
    private const int MAX_TRIANGLES_COUNT = 1000000;
    private const int FLOATS_PER_TRIANGLE = 9;
    private const int VERTICES_PER_TRIANGLE = 3;
    private const int FLOATS_PER_POINT = 3;
    private const int HEADER_SIZE = 80;

    private static readonly float[] _5bitsToFloat = new float[32];

    static STLImporter()
    {
        for (int i = 0; i < 32; i++)
        {
            _5bitsToFloat[i] = i / 31.0f;
        }
    }

    public static unsafe STLModel? LoadModel(string path, RenderSettings settings = RenderSettings.All)
    {
        settings &= RenderSettings.VertexColors;

        if (File.Exists(path) == false)
        {
            SystemCalls.PrintMessage($"Error. STL-file {path} does not exists", MessageTypes.ErrorMessage);
            return null;
        }

        using FileStream stream = File.OpenRead(path);
        using BinaryReader reader = new(stream);

        if (Encoding.ASCII.GetString(reader.ReadBytes(5)) == "solid" &&
            (reader.BaseStream.Length - HEADER_SIZE - sizeof(uint)) % 50 != 0)
        {
            SystemCalls.PrintMessage("Error. ASCII STL not supported yet", MessageTypes.ErrorMessage);
            return null;
        }

        reader.BaseStream.Seek(0, SeekOrigin.Begin);

        var header = Encoding.ASCII.GetString(reader.ReadBytes(HEADER_SIZE)).Trim(' ', '\0');
        var trianglesCount = reader.ReadUInt32();

        if (trianglesCount > MAX_TRIANGLES_COUNT)
        {
            SystemCalls.PrintMessage($"Error. STL model is too big ({trianglesCount}) triangles. Max supported triangles: 1 000 000", MessageTypes.ErrorMessage);
            return null;
        }

        PrimitiveData primitive = new()
        {
            Vertices = new byte[trianglesCount * FLOATS_PER_TRIANGLE * sizeof(float)],
            Options = new() { Count = (int)trianglesCount }
        };

        byte[] normal = new byte[FLOATS_PER_POINT * sizeof(float)];
        byte[] vertices = new byte[FLOATS_PER_TRIANGLE * sizeof(float)];
        byte[] colors = new byte[(FLOATS_PER_POINT + 1) * sizeof(float)];
        float[] colorsFloat = new float[4];

        bool firstRead = true;
        bool hasColors = false;

        primitive.Normals = new byte[trianglesCount * FLOATS_PER_TRIANGLE * sizeof(float)];

        try
        {
            for (int i = 0; i < trianglesCount; i++)
            {
                reader.Read(normal, 0, normal.Length);

                for (int j = 0; j < VERTICES_PER_TRIANGLE; j++)
                {
                    var offsetN = i * FLOATS_PER_TRIANGLE * sizeof(float) + j * VERTICES_PER_TRIANGLE * sizeof(float);
                    Buffer.BlockCopy(normal, 0, primitive.Normals!, offsetN, FLOATS_PER_POINT * sizeof(float));
                }

                reader.Read(vertices, 0, vertices.Length);

                var offsetV = i * FLOATS_PER_TRIANGLE * sizeof(float);
                Buffer.BlockCopy(vertices, 0, primitive.Vertices, offsetV, FLOATS_PER_TRIANGLE * sizeof(float));

                if ((settings & RenderSettings.VertexColors) != 0)
                {
                    ushort attributeByteCount = reader.ReadUInt16();

                    if (attributeByteCount >> 15 != 0)
                    {
                        if (firstRead)
                        {
                            primitive.Colors = new byte[trianglesCount * (FLOATS_PER_POINT + 1) * VERTICES_PER_TRIANGLE * sizeof(float)];
                            firstRead = false;
                        }

                        hasColors = true;

                        colorsFloat[0] = _5bitsToFloat[(attributeByteCount >> 10) & 0x1F];
                        colorsFloat[1] = _5bitsToFloat[(attributeByteCount >> 5) & 0x1F];
                        colorsFloat[2] = _5bitsToFloat[attributeByteCount & 0x1F];
                        colorsFloat[3] = 1.0f;

                        Buffer.BlockCopy(colorsFloat, 0, colors, 0, colors.Length);

                        for (int j = 0; j < 3; j++)
                        {
                            var offsetC = i * (FLOATS_PER_POINT + 1) * VERTICES_PER_TRIANGLE * sizeof(float)
                                        + j * (FLOATS_PER_POINT + 1) * sizeof(float);

                            Buffer.BlockCopy(colors, 0, primitive.Colors!, offsetC, (FLOATS_PER_POINT + 1) * sizeof(float));
                        }
                    }
                }
                else
                {
                    reader.BaseStream.Seek(sizeof(ushort), SeekOrigin.Current);
                }
            }

            if (reader.BaseStream.Length != reader.BaseStream.Position)
                throw new SystemException();
        }
        catch (SystemException)
        {
            SystemCalls.PrintMessage($"Error. STL-file {path} is not valid", MessageTypes.ErrorMessage);
            return null;
        }

        Vector3 min = new(float.MaxValue);
        Vector3 max = new(float.MinValue);

        fixed (void* vertexPtr = &primitive.Vertices[0])
        {
            float* floatPtr = (float*)vertexPtr;
            int vertexCount = primitive.Vertices.Length / (3 * sizeof(float));

            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 currentVertex = new(floatPtr[0], floatPtr[1], floatPtr[2]);
                min = Vector3.ComponentMin(min, currentVertex);
                max = Vector3.ComponentMax(max, currentVertex);

                floatPtr += 3;
            }
        }

        primitive.Box = new BoundingBox(min, max);

        if (hasColors == false)
            settings &= ~RenderSettings.VertexColors;

        STLModel result = new(new MeshData([primitive]), settings) { Path = path };

        return result;
    }
}
