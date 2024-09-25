using OpenTK.Mathematics;

namespace Game_Engine.Core
{
    public readonly struct Triangle
    {
        public Vector3 Color { get; } = new(0.5f, 0.5f, 0.5f);
        public Vector3 NormalVector { get; }
        public Vector3[] Vertices { get; }
        public ushort AttributeByteCount { get; }

        public Triangle(Vector3 normalVector, Vector3[] vertexes, ushort attributeByteCount)
        {
            if (vertexes.Length != 3)
                throw new ArgumentException("Передано неверное количество вершин");

            NormalVector = normalVector;
            Vertices = vertexes;
            AttributeByteCount = attributeByteCount;

            var str = Convert.ToString(attributeByteCount, 2);
            str = new string('0', sizeof(ushort) * 8 - str.Length) + str;

            if (str[^1] != '0')
            {
                Color = new((float)Convert.ToByte(str[..5], 2) / 32,
                            (float)Convert.ToByte(str[5..10], 2) / 32,
                            (float)Convert.ToByte(str[10..^1], 2) / 32);

            }
        }
    }

    public class STLModel
    {
        public string Header { get; }
        public uint TrianglesCount { get; }
        public Triangle[] Triangles { get; }

        public STLModel(string path)
        {
            using BinaryReader br = new(File.OpenRead(path));

            Header = new string(br.ReadChars(80)).Trim('\0');
            TrianglesCount = br.ReadUInt32();
            Triangle[] triangles = new Triangle[TrianglesCount];

            for (int i = 0; i < triangles.Length; i++)
            {
                triangles[i] = BuildTriangle(br);
            }

            Triangles = triangles;

            if (Triangles[^1].Vertices is null)
                throw new FileLoadException("Ошибка, поврежденный файл");
        }

        private static Triangle BuildTriangle(BinaryReader br)
        {
            Vector3 normalVector;
            Vector3[] vertexes = new Vector3[3];
            ushort attributeByteCount;
            float[] triangleProps = new float[12];

            for (int i = 0; i < 12; i++)
            {
                triangleProps[i] = br.ReadSingle();
            }

            for (int i = 0; i < 3; i++)
            {
                vertexes[i] = new Vector3(x: triangleProps[3 * i + 3],
                                          y: triangleProps[3 * i + 4],
                                          z: triangleProps[3 * i + 5]);
            }

            normalVector = new(x: triangleProps[0],
                               y: triangleProps[1],
                               z: triangleProps[2]);

            attributeByteCount = br.ReadUInt16();

            return new Triangle(normalVector, vertexes, attributeByteCount);
        }
    }
}
