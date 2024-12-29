namespace Game_Engine.Core;

internal static class Mathematics
{
    public static float DegreesToRadians(float degrees) => degrees * (float.Pi / 180);
    public static float RadiansToDegrees(float radians) => radians * 180 / float.Pi;

    public static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        else if (value > max) return max;
        else return value;
    }

    public static float[,] MultiplyMatrices(float[,] m1, float[,] m2)
    {
        var result = new float[m1.GetLength(0), m2.GetLength(1)];

        if (m1.GetLength(1) != m2.GetLength(0))
            throw new InvalidOperationException("Матрицы невозможно перемножить");

        for (int i = 0; i < m1.GetLength(0); i++)
        {
            for (int j = 0; j < m2.GetLength(1); j++)
            {
                for (int k = 0; k < m1.GetLength(1); k++)
                {
                    result[i, j] += m1[i, k] * m2[k, j];
                }
            }
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
