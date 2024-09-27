using Game_Engine.Enums;
using OpenTK.Mathematics;

namespace Game_Engine.Core
{
    internal class GameObjectBase3D(STLModel model)
    {
        private Vector3 _scale = new(1f, 1f, 1f);

        public STLModel Model { get; } = model;
        public Vector3 Rotation { get; set; }
        public Vector3 Position { get; set; }
        public int Handle { get; set; }

        public Vector3 Scale
        {
            get => _scale;

            set
            {
                _scale = value;
                for (int i = 0; i < Model.TrianglesCount * 3; i++)
                {
                    Model.SetData(AttribTypes.Vertex, i, Vector3.Multiply(Model.GetData(AttribTypes.Vertex, i), value));
                }
            }
        }
    }
}
