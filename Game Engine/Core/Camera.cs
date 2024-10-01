using OpenTK.Mathematics;

namespace Game_Engine.Core;

internal class Camera
{
    private Vector3 _position = new (0f, 0f, 200f);

    public Camera(Vector2i windowSize)
    {       
        ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(Mathematics.DegreesToRadians(45f),
                           (float)windowSize.X / windowSize.Y, 1f, 5000f);
        CalculateVectors();
    }

    public Vector3 Up { get; set; }
    public Vector3 Right { get; set; }
    public Vector3 Direction { get; set; }
    public Matrix4 ProjectionMatrix { get; set;}
    public Matrix4 ViewMatrix { get; set;}
    public Vector3 Position
    {
        get => _position;

        set
        {
            _position = value;
            CalculateVectors();
        }
    }

    private void CalculateVectors()
    {
        Vector3 target = Vector3.Zero;

        Direction = Vector3.Normalize(_position - target);
        Right = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, Direction));
        Up = Vector3.Cross(Direction, Right);
        ViewMatrix = Matrix4.LookAt(_position, Position - Vector3.UnitZ, Vector3.UnitY);
    }
}
