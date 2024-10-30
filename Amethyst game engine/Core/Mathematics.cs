using System.Numerics;

namespace Amethyst_game_engine.Core;

internal static class Mathematics
{
    public static float DegreesToRadians(float degrees) => degrees * (float.Pi / 180);
    public static float RadiansToDegrees(float radians) => radians * 180 / float.Pi;

    public static float[,] ConvertQuaternionToMatrix(float x, float y, float z, float w)
    {
        return new float[4, 4]
        {
            { 1 - 2 * (y * y + z * z), 2 * (x * y - w * z), 2 * (x * z + w * y), 0 },
            { 2 * (x * y + w * z), 1 - 2 * (x * x + z * z), 2 * (y * z - w * x), 0 },
            { 2 * (x * z - w * y), 2 * (y * z + w * x), 1 - 2 * (x * x + y * y), 0 },
            { 0,                   0,                       0,                   1 },
        };
    }

    public static float[,] GetMatrixFromArray(float[] array, int colums, int rows)
    {
        float[,] result = new float[rows, colums];

        if (array.Length == colums * rows)
        {
            var index = 0;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < colums; col++)
                {
                    result[row, col] = array[index++];
                }
            }

            return result;
        }
        else
        {
            throw new ArgumentException("Error. The number of elements in the array is less than the size of the matrix");
        }
    }

    public static float[,] GetMatrixFromArray(float[] array)
    {
        var size = MathF.Sqrt(array.Length);

        if (float.IsInteger(size))
            return GetMatrixFromArray(array, (int)size, (int)size);
        else
            throw new ArgumentException("Error. The matrix is not square or the number of elements in the array is incorrect");
    }

    public static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        else if (value > max) return max;
        else return value;
    }

    public static float[,] MultiplyMatrices(float[,] m1, float[,] m2)
    {
        var result = new float[m1.GetLength(0), m2.GetLength(1)];

        if (m1.GetLength(1) == m2.GetLength(0))
        {
            for (int row = 0; row < m1.GetLength(0); row++)
            {
                for (int col = 0; col < m2.GetLength(1); col++)
                {
                    for (int i = 0; i < m1.GetLength(1); i++)
                    {
                        result[row, col] += m1[row, i] * m2[i, col];
                    }
                }
            }

            return result;
        }
        else
        {
            throw new ArgumentException("Error. Matrices cannot be multiplied");
        }
    }

    public static float[,] CreateTranslationMatrix(float x, float y, float z)
    {
        return new float[,]
        {
            { 1, 0, 0, x },
            { 0, 1, 0, y },
            { 0, 0, 1, z },
            { 0, 0, 0, 1 },
        };
    }

    public static float[,] CreateScaleMatrix(float x, float y, float z)
    {
        return new float[,]
        {
            { x, 0, 0, 0 },
            { 0, y, 0, 0 },
            { 0, 0, z, 0 },
            { 0, 0, 0, 1 },
        };
    }

    public static float[,] CreateRotationXMatrix(float angle)
    {
        angle = DegreesToRadians(angle);

        return new float[,]
        {
            { 1,            0,                       0,           0 },
            { 0, (float)Math.Cos(angle), -(float)Math.Sin(angle), 0 },
            { 0, (float)Math.Sin(angle),  (float)Math.Cos(angle), 0 },
            { 0,            0,                       0,           1 },
        };
    }
}
