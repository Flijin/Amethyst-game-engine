using OpenTK.Mathematics;

namespace Game_Engine.Core
{
    internal class GameObjectBase3D
    {
        private Vector3 _scale = new(1f, 1f, 1f);

        public Vector3 Rotation { get; set; }
        public Vector3 Position { get; set; }
        public STLModel Model { get; }

        public Vector3 Scale
        {
            get => _scale;

            set
            {
                _scale = value;

                for (int triangle = 0; triangle < Model.TrianglesCount; triangle++)
                {
                    for (int vertex = 0; vertex < 0; vertex++)
                    {
                        Model[triangle, vertex] = Vector3.Multiply(Model[triangle, vertex], value);
                    }
                }
            }
        }

        public GameObjectBase3D(STLModel model)
        {
            Model = model;
        }
    }
}
