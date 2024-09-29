using OpenTK.Mathematics;

namespace Game_Engine.Core;

internal class Camera(Vector2i windowSize)
{
    public Matrix4 ProjectionMatrix { get; } =
        Matrix4.CreatePerspectiveFieldOfView(Mathematics.DegreesToRadians(45f),
            (float)windowSize.X / windowSize.Y, 1f, 10000f);

    public float[,] ViewMatrix { get; } = Mathematics.CreateTranslationMatrix(0f, 0f, -200f);
}
