using OpenTK.Mathematics;

namespace Game_Engine.Core
{
    internal class GameObjectBase3D(STLModel model)
    {
        private Vector3 _scale;
        private Vector3 _position;
        private float _rotation;

        private float[,] _modelMatrix =
        {
            { 1, 0, 0, 0 },
            { 0, 1, 0, 0 },
            { 0, 0, 1, 0 },
            { 0, 0, 0, 1 },
        };

        public string Name { get; set; } = string.Empty;
        public float[,] ModelMatrix => _modelMatrix;
        public STLModel Model { get; } = model;
        public int VAO { get; set; }
        public int VBO {  get; set; }

        public Vector3 Scale
        {
            get => _scale;

            set
            {
                _scale = value;
                _modelMatrix = Mathematics.MultiplyMatrices(_modelMatrix,
                               Mathematics.CreateScaleMatrix(value.X, value.Y, value.Z));
            }
        }

        public Vector3 Position
        {
            get => _position;

            set
            {
                _position = value;
                _modelMatrix = Mathematics.MultiplyMatrices(_modelMatrix,
                               Mathematics.CreateTranslationMatrix(value.X, value.Y, value.Z));
            }
        }

        public float Rotation
        {
            get => _rotation;

            set
            {
                _rotation = value;
                _modelMatrix = Mathematics.MultiplyMatrices(_modelMatrix,
                               Mathematics.CreateRotationXMatrix(
                               Mathematics.DegreesToRadians(value)));
            }
        }
    }
}
