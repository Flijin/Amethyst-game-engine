using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core.GameObjects.Components;

public readonly struct BoundingBox(Vector3 min, Vector3 max)
{
    public Vector3 Min { get; } = min;
    public Vector3 Max { get; } = max;

    public readonly Vector3 Center => (Min + Max) / 2;
    public readonly Vector3 Size => Max - Min;

    public readonly Vector3[] GetCorners()
    {
        return
        [
            new(Min.X, Min.Y, Min.Z),
            new(Max.X, Min.Y, Min.Z),
            new(Min.X, Max.Y, Min.Z),
            new(Max.X, Max.Y, Min.Z),
            
            new(Min.X, Min.Y, Max.Z),
            new(Max.X, Min.Y, Max.Z),
            new(Min.X, Max.Y, Max.Z),
            new(Max.X, Max.Y, Max.Z)
        ];
    }

    public static unsafe BoundingBox TransformBox(BoundingBox box, float* matrix)
    {
        Vector3[] corners = box.GetCorners();

        Vector3 min = new(float.MaxValue);
        Vector3 max = new(float.MinValue);

        float m11 = matrix[0]; float m12 = matrix[1]; float m13 = matrix[2]; float m14 = matrix[3];
        float m21 = matrix[4]; float m22 = matrix[5]; float m23 = matrix[6]; float m24 = matrix[7];
        float m31 = matrix[8]; float m32 = matrix[9]; float m33 = matrix[10]; float m34 = matrix[11];

        foreach (var corner in corners)
        {
            float x = m11 * corner.X + m12 * corner.Y + m13 * corner.Z + m14;
            float y = m21 * corner.X + m22 * corner.Y + m23 * corner.Z + m24;
            float z = m31 * corner.X + m32 * corner.Y + m33 * corner.Z + m34;

            Vector3 transformed = new(x, y, z);

            min = Vector3.ComponentMin(min, transformed);
            max = Vector3.ComponentMax(max, transformed);
        }

        return new BoundingBox(min, max);
    }
}
