using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using Numerics = System.Numerics;

namespace Amethyst_game_engine.Core.Utilities;

public static class Mathematics
{
    public const int MATRIX_SIZE = sizeof(float) * 16;
    public static unsafe readonly float* IDENTITY_MATRIX;

    unsafe static Mathematics()
    {
        IDENTITY_MATRIX = (float*)Marshal.AllocHGlobal(16 * sizeof(float));
        Unsafe.InitBlock(IDENTITY_MATRIX, 0, MATRIX_SIZE);

        for (int i = 0; i < 4; i++)
        {
            IDENTITY_MATRIX[4 * i + i] = 1.0f;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DegreesToRadians(float degrees) => degrees * (float.Pi / 180);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RadiansToDegrees(float radians) => radians * 180 / float.Pi;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Clamp<T>(T value, T min, T max) where T : INumber<T>
    {
        if (value < min) return min;
        else if (value > max) return max;
        else return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void CreateTranslationMatrix4(float x, float y, float z, float* res)
    {
        float* temp = stackalloc float[16]
        {
             1.0f, 0.0f, 0.0f, x,
             0.0f, 1.0f, 0.0f, y,
             0.0f, 0.0f, 1.0f, z,
             0.0f, 0.0f, 0.0f, 1.0f,
        };

        Buffer.MemoryCopy(temp, res, MATRIX_SIZE, MATRIX_SIZE);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float[] CreateTranslationMatrix4(float x, float y, float z)
    {
        return
        [
             1.0f, 0.0f, 0.0f, x,
             0.0f, 1.0f, 0.0f, y,
             0.0f, 0.0f, 1.0f, z,
             0.0f, 0.0f, 0.0f, 1.0f,
        ];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void CreateScaleMatrix4(float x, float y, float z, float* res)
    {
        float* temp = stackalloc float[16]
        {
            x,    0.0f, 0.0f, 0.0f,
            0.0f, y,    0.0f, 0.0f,
            0.0f, 0.0f, z,    0.0f,
            0.0f, 0.0f, 0.0f, 1.0f,
        };

        Buffer.MemoryCopy(temp, res, MATRIX_SIZE, MATRIX_SIZE);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public static float[] CreateScaleMatrix4(float x, float y, float z)
    {
        return
        [
            x,    0.0f, 0.0f, 0.0f,
            0.0f, y,    0.0f, 0.0f,
            0.0f, 0.0f, z,    0.0f,
            0.0f, 0.0f, 0.0f, 1.0f,
        ];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CalculateLightRadius(float constant, float linear, float quadratic)
    {
        float threshold = 0.025f;
        float target = 1.0f / threshold;

        float a = quadratic;
        float b = linear;
        float c = constant - target;

        float discriminant = b * b - 4 * a * c;

        float sqrtDisc = MathF.Sqrt(discriminant);
        float d1 = (-b + sqrtDisc) / (2 * a);
        float d2 = (-b - sqrtDisc) / (2 * a);

        float radius = MathF.Max(d1, d2);

        return radius > 0.0f ? radius : float.PositiveInfinity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OpenTK.Mathematics.Vector3 GetDirectionFromEuler(float yaw, float pitch)
    {
        float pitchRad = MathHelper.DegreesToRadians(pitch);
        float yawRad = MathHelper.DegreesToRadians(yaw);

        float x = (float)(Math.Cos(pitchRad) * Math.Cos(yawRad));
        float y = (float)(Math.Sin(pitchRad));
        float z = (float)(Math.Cos(pitchRad) * Math.Sin(yawRad));

        return new OpenTK.Mathematics.Vector3(x, y, z).Normalized();
    }

    public static unsafe void MultiplyMatrices4(float* m1, float* m2, float* res)
    {
        for (int i = 0; i < 4; ++i)
        {
            Numerics.Vector4 row = new(m1[i * 4], m1[i * 4 + 1], m1[i * 4 + 2], m1[i * 4 + 3]);

            for (int j = 0; j < 4; ++j)
            {
                Numerics.Vector4 col = new(m2[j], m2[4 + j], m2[8 + j], m2[12 + j]);
                res[i * 4 + j] = Numerics.Vector4.Dot(row, col);
            }
        }
    }

    public static void TransposeMatrix4(float[] matrix)
    {
        float[] temp = new float[16];
        Array.Copy(matrix, temp, 16);

        for (int i = 0; i < 4; i++)
        {
            matrix[i * 4] = temp[i];
            matrix[i * 4 + 1] = temp[4 + i];
            matrix[i * 4 + 2] = temp[8 + i];
            matrix[i * 4 + 3] = temp[12 + i];
        }
    }
}
