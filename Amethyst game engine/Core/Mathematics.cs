using System.Numerics;

namespace Amethyst_game_engine.Core;

public static class Mathematics
{
    public static readonly float[,] UNIT_MATRIX =
    {
        { 1, 0, 0, 0 },
        { 0, 1, 0, 0 },
        { 0, 0, 1, 0 },
        { 0, 0, 0, 1 }
    };

    public static float DegreesToRadians(float degrees) => degrees * (float.Pi / 180);
    public static float RadiansToDegrees(float radians) => radians * 180 / float.Pi;

    public static T[,] CreateMatrixFromArray<T>(T[] array, bool columnForm) where T : INumber<T>
    {
        var matrixSize = MathF.Sqrt(array.Length);

        if (float.IsInteger(matrixSize))
        {
            T[,] result = new T[(int)matrixSize, (int)matrixSize];
            var index = 0;

            if (columnForm)
            {
                for (int row = 0; row < matrixSize; row++)
                {
                    for (int col = 0; col < matrixSize; col++)
                    {
                        result[col, row] = array[index++];
                    }
                }
            }
            else
            {
                for (int row = 0; row < matrixSize; row++)
                {
                    for (int col = 0; col < matrixSize; col++)
                    {
                        result[row, col] = array[index++];
                    }
                }
            }

            return result;
        }
        else
        {
            throw new ArgumentException("Error. The number of elements in the array is less than the size of the matrix");
        }
    }

    public static T Clamp<T>(T value, T min, T max) where T : INumber<T>
    {
        if (value < min) return min;
        else if (value > max) return max;
        else return value;
    }

    public static T[,] MultiplyMatrices<T>(T[,] m1, T[,] m2) where T : INumber<T>
    {
        var result = new T[m1.GetLength(0), m2.GetLength(1)];

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
        }
        else
        {
            throw new ArgumentException("Error. Matrices cannot be multiplied");
        }

        return result;
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
}
