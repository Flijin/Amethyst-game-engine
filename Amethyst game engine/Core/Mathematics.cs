using System.Numerics;
using System.Runtime.InteropServices;

namespace Amethyst_game_engine.Core;

public static class Mathematics
{
    public const int MATRIX_SIZE = sizeof(float) * 16;
    public static unsafe readonly float* IDENTITY_MATRIX;

    unsafe static Mathematics()
    {
        IDENTITY_MATRIX = (float*)Marshal.AllocHGlobal(16 * sizeof(float));

        for (int i = 0; i < 4; i++)
        {
            IDENTITY_MATRIX[4 * i + i] = 1;
        }
    }

    public static float DegreesToRadians(float degrees) => degrees * (float.Pi / 180);
    public static float RadiansToDegrees(float radians) => radians * 180 / float.Pi;

    public static unsafe void TransposeMatrix4(float* matrix)
    {
        for (int i = 0; i < 4; i++)
        {
            matrix[i * 4] = matrix[i];
            matrix[i * 4 + 1] = matrix[4 + i];
            matrix[i * 4 + 2] = matrix[8 + i];
            matrix[i * 4 + 3] = matrix[12 + i];
        }
    }

    public static T Clamp<T>(T value, T min, T max) where T : INumber<T>
    {
        if (value < min) return min;
        else if (value > max) return max;
        else return value;
    }

    public static unsafe void MultiplyMatrices4(float* m1, float* m2, float* res)
    {
        for (int i = 0; i < 4; ++i)
        {
            Vector4 row = new(m1[i * 4], m1[i * 4 + 1], m1[i * 4 + 2], m1[i * 4 + 3]);

            for (int j = 0; j < 4; ++j)
            {
                Vector4 col = new(m2[j], m2[4 + j], m2[8 + j], m2[12 + j]);
                res[i * 4 + j] = Vector4.Dot(row, col);
            }
        }
    }

    public static unsafe void CreateTranslationMatrix4(float x, float y, float z, float* res)
    {
        float* temp = stackalloc float[16]
        {
             1, 0, 0, x,
             0, 1, 0, y,
             0, 0, 1, z,
             0, 0, 0, 1,
        };

        Buffer.MemoryCopy(temp, res, MATRIX_SIZE, MATRIX_SIZE);
    }

    public static unsafe void CreateScaleMatrix4(float x, float y, float z, float* res)
    {
        float* temp = stackalloc float[16]
        {
            x, 0, 0, 0,
            0, y, 0, 0,
            0, 0, z, 0,
            0, 0, 0, 1,
        };

        Buffer.MemoryCopy(temp, res, MATRIX_SIZE, MATRIX_SIZE);
    }
}
