using Game_Engine.Enums;
using OpenTK.Mathematics;

namespace Game_Engine.Core
{
    internal class STLModel
    {
        private const int COLOR_OFFSET = 3;
        private const int STRIDE = 3;

        /// <summary>
        ///     Structure:
        ///         <para>Contains vertices of polygons in local space</para>
        ///         <para>(X1, Y1, Z1, X2 ...)</para>
        /// </summary>
        public float[] Vertices { get; }
        /// <summary>
        ///     Structure:
        ///         <para>
        ///             Contains vertices of normal vector and 5-bit corors in normalized form (0.0 - 1.0)
        ///         </para>
        ///         <para>(X1, Y1, Z1, R1, G1, B1, X2 ...)</para>
        /// </summary>
        public float[] RenderData { get; }

        public string Header { get; }
        public uint TrianglesCount { get; }
        public Vector3 DefaultColor { get; init; } = new Vector3(0.5f, 0.5f, 0.5f);

        public STLModel(string path)
        {
            var vIndex = 0;
            var rIndex = 0;

            using BinaryReader br = new(File.OpenRead(path));

            Header = new string(br.ReadChars(80)).Trim('\0');
            TrianglesCount = br.ReadUInt32();
            Vertices = new float[TrianglesCount * 9];
            RenderData = new float[TrianglesCount * 6];

            for (int i = 0; i < TrianglesCount; i++)
            {
                for (int vectors = 0; vectors < 3; vectors++)
                {
                    RenderData[rIndex++] = br.ReadSingle();
                }

                for (int vectors = 0; vectors < 9; vectors++)
                {
                    Vertices[vIndex++] = br.ReadSingle();
                }

                ushort attributeByteCount = br.ReadUInt16();

                Vector3 color;
                var str = Convert.ToString(attributeByteCount, 2);
                str = new string('0', sizeof(ushort) * 8 - str.Length) + str;

                if (str[^1] != '0')
                {
                    color = new((float)Convert.ToByte(str[..5], 2) / 32,
                                (float)Convert.ToByte(str[5..10], 2) / 32,
                                (float)Convert.ToByte(str[10..^1], 2) / 32);

                }
                else
                {
                    color = DefaultColor;
                }

                for (int colorIndex = 0; colorIndex < 3; colorIndex++)
                {
                    RenderData[rIndex++] = color[colorIndex];
                }
            }
        }

        public Vector3 GetData(AttribTypes type, int index)
        {
            Vector3 result = new();

            switch (type)
            {
                case AttribTypes.Vertex:
                    for (int i = 0; i < 3; i++)
                    {
                        result[i] = Vertices[STRIDE * index + i];
                    }
                    break;
                case AttribTypes.Color:
                    for (int i = 0; i < 3; i++)
                    {
                        result[i] = RenderData[STRIDE * index * 2 + COLOR_OFFSET + i];
                    }
                    break;
                case AttribTypes.Normal:
                    for (int i = 0; i < 3; i++)
                    {
                        result[i] = RenderData[STRIDE * index * 2 + i];
                    }
                    break;
            }

            return result;
        }

        public void SetData(AttribTypes type, int index, Vector3 vector)
        {
            switch (type)
            {
                case AttribTypes.Vertex:
                    for (int i = 0; i < 3; i++)
                    {
                        Vertices[STRIDE * index + i] = vector[i];
                    }
                    break;
                case AttribTypes.Color:             
                    for (int i = 0; i < 3; i++)
                    {
                        RenderData[STRIDE * index * 2 + COLOR_OFFSET + i] = vector[i];
                    }
                    break;
                case AttribTypes.Normal:
                    for (int i = 0; i < 3; i++)
                    {
                        RenderData[STRIDE * index * 2 + i] = vector[i];
                    }
                    break;
            }
        }
    }
}
