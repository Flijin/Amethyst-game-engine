namespace Amethyst_game_engine.Core;

internal readonly struct Quaternion
{
    public readonly float x;
    public readonly float y;
    public readonly float z;
    public readonly float w;

    public Quaternion(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public Quaternion(float[] values)
    {
        x = values[0]; y = values[1];
        z = values[2]; w = values[3];
    }

    public Quaternion(float pitch, float yaw, float roll)
    {
        var radPitch = Mathematics.DegreesToRadians(pitch);
        var radYaw = Mathematics.DegreesToRadians(yaw);
        var radRoll = Mathematics.DegreesToRadians(roll);

        float cy = MathF.Cos(radYaw * 0.5f);
        float sy = MathF.Sin(radYaw * 0.5f);
        float cp = MathF.Cos(radPitch * 0.5f);
        float sp = MathF.Sin(radPitch * 0.5f);
        float cr = MathF.Cos(radRoll * 0.5f);
        float sr = MathF.Sin(radRoll * 0.5f);

        x = sr * cp * cy - cr * sp * sy;
        y = cr * sp * cy + sr * cp * sy;
        z = cr * cp * sy - sr * sp * cy;
        w = cr * cp * cy + sr * sp * sy;
    }

    public unsafe void GetRotationMatrix(float* res) => ConvertQuaternionToMatrix(x, y, z, w, res);
    public static unsafe void ConvertQuaternionToMatrix(Quaternion q, float* res) => ConvertQuaternionToMatrix(q.x, q.y, q.z, q.w, res);

    public static unsafe void ConvertQuaternionToMatrix(float x, float y, float z, float w, float* res)
    {
        float* temp = stackalloc float[16]
        {
            1 - 2 * (y * y + z * z), 2 * (x * y - w * z), 2 * (x * z + w * y), 0,
            2 * (x * y + w * z), 1 - 2 * (x * x + z * z), 2 * (y * z - w * x), 0,
            2 * (x * z - w * y), 2 * (y * z + w * x), 1 - 2 * (x * x + y * y), 0,
            0,                   0,                       0,                   1
        };

        Buffer.MemoryCopy(temp, res, Mathematics.MATRIX_SIZE, Mathematics.MATRIX_SIZE);
    }
}
