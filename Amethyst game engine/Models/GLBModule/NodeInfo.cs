using Amethyst_game_engine.Core;
using System.Runtime.InteropServices;

namespace Amethyst_game_engine.Models.GLBModule;

internal unsafe struct NodeInfo : IDisposable
{
    private float* _translationMatrix = null;
    private float* _rotationMatrix = null;
    private float* _scaleMatrix = null;
    private readonly int _index;

    public readonly int Index => _index;

    public string? Name { get; }
    public int? Mesh { get; }
    public int? Skin { get; }
    public int[]? Children { readonly get; set; }
    public float* LocalMatrix { readonly get; private set; } = null;
    private float* PreviousMatrix { readonly get; set; } = null;
    public float* GlobalMatrix { readonly get; private set; } = null;

    public NodeInfo(Dictionary<string, object> nodePresentation, int index)
    {
        _index = index;

        if (nodePresentation.TryGetValue("name", out object? name))
            Name = (string)name;

        if (nodePresentation.TryGetValue("children", out object? children))
            Children = [.. ((object[])children).Cast<int>()];

        if (nodePresentation.TryGetValue("mesh", out object? mesh))
            Mesh = (int)mesh;

        if (nodePresentation.TryGetValue("skin", out object? skin))
            Skin = (int)skin;

        if (nodePresentation.TryGetValue("matrix", out object? matrix))
        {
            var temp = ((object[])matrix).Select(el => el = Convert.ToSingle(el))
                                         .Cast<float>()
                                         .ToArray();

            LocalMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);

            fixed (float* ptr = temp)
            {
                Buffer.MemoryCopy(ptr, LocalMatrix, Mathematics.MATRIX_SIZE, Mathematics.MATRIX_SIZE);
            }

            Mathematics.TransposeMatrix4(LocalMatrix);

            return;
        }

        if (nodePresentation.TryGetValue("translation", out object? translation))
        {
            var t = ((object[])translation).Select(el => el = Convert.ToSingle(el))
                            .Cast<float>()
                            .ToArray();

            _translationMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);
            LocalMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);

            Mathematics.CreateTranslationMatrix4(t[0], t[1], t[2], _translationMatrix);
            Buffer.MemoryCopy(_translationMatrix, LocalMatrix, Mathematics.MATRIX_SIZE, Mathematics.MATRIX_SIZE);
        }

        if (nodePresentation.TryGetValue("rotation", out object? rotation))
        {
            var r = ((object[])rotation).Select(el => el = Convert.ToSingle(el))
                                        .Cast<float>()
                                        .ToArray();

            _rotationMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);

            Quaternion.ConvertQuaternionToMatrix
            (
                r[0], r[1], r[2], r[3], _rotationMatrix
            );

            if (LocalMatrix is not null)
            {
                float* temp = stackalloc float[16];
                Buffer.MemoryCopy(LocalMatrix, temp, Mathematics.MATRIX_SIZE, Mathematics.MATRIX_SIZE);

                Mathematics.MultiplyMatrices4(temp, _rotationMatrix, LocalMatrix);
            }
            else
            {
                LocalMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);
                Buffer.MemoryCopy(_rotationMatrix, LocalMatrix, Mathematics.MATRIX_SIZE, Mathematics.MATRIX_SIZE);
            }
        }

        if (nodePresentation.TryGetValue("scale", out object? scale))
        {
            var s = ((object[])scale).Select(el => el = Convert.ToSingle(el))
                .Cast<float>()
                .ToArray(); 

            _scaleMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);

            Mathematics.CreateScaleMatrix4
            (
                s[0], s[1], s[2], _scaleMatrix
            );

            if (LocalMatrix is not null)
            {
                float* temp = stackalloc float[16];
                Buffer.MemoryCopy(LocalMatrix, temp, Mathematics.MATRIX_SIZE, Mathematics.MATRIX_SIZE);

                Mathematics.MultiplyMatrices4(temp, _scaleMatrix, LocalMatrix);
            }
            else
            {
                LocalMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);
                Buffer.MemoryCopy(_scaleMatrix, LocalMatrix, Mathematics.MATRIX_SIZE, Mathematics.MATRIX_SIZE);
            }
        }
    }

    public void CalculateGlobalMatrix(float* matrix)
    {
        PreviousMatrix = matrix;

        if (matrix is not null)
        {
            GlobalMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);

            if (LocalMatrix is null)
                Buffer.MemoryCopy(matrix, GlobalMatrix, Mathematics.MATRIX_SIZE, Mathematics.MATRIX_SIZE);
            else
                Mathematics.MultiplyMatrices4(matrix, LocalMatrix, GlobalMatrix);
        }
        else if (LocalMatrix is not null)
        {
            GlobalMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);
            Buffer.MemoryCopy(LocalMatrix, GlobalMatrix, Mathematics.MATRIX_SIZE, Mathematics.MATRIX_SIZE);
        }
    }

    public void CalculateTRS(float* t, float* r, float* s)
    {
        float* resultMatrix = null;

        if (t is not null)
        {
            if (_translationMatrix is null)
                _translationMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);

            Mathematics.CreateTranslationMatrix4(t[0], t[1], t[2], _translationMatrix);
        }
        if (r is not null)
        {
            if (_rotationMatrix is null)
                _rotationMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);

            Quaternion.ConvertQuaternionToMatrix(r[0], r[1], r[2], r[3], _rotationMatrix);
        }
        if (s is not null)
        {
            if (_scaleMatrix is null)
                _scaleMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);

            Mathematics.CreateScaleMatrix4(s[0], s[1], s[2], _scaleMatrix);
        }

        if (_translationMatrix is not null)
        {
            resultMatrix = _translationMatrix;
        }
        if (_rotationMatrix is not null)
        {
            if (resultMatrix is not null)
            {
                float* temp = stackalloc float[16];
                Buffer.MemoryCopy(resultMatrix, temp, Mathematics.MATRIX_SIZE, Mathematics.MATRIX_SIZE);
                Mathematics.MultiplyMatrices4(temp, _rotationMatrix, resultMatrix);
            }
            else
                resultMatrix = _rotationMatrix;
        }
        if (_scaleMatrix is not null)
        {
            if (resultMatrix is not null)
            {
                float* temp = stackalloc float[16];
                Buffer.MemoryCopy(resultMatrix, temp, Mathematics.MATRIX_SIZE, Mathematics.MATRIX_SIZE);
                Mathematics.MultiplyMatrices4(temp, _scaleMatrix, resultMatrix);
            }
            else
                resultMatrix = _scaleMatrix;
        }

        LocalMatrix = resultMatrix;

        if (resultMatrix is not null && PreviousMatrix is not null)
        {
            if (GlobalMatrix is null)
                GlobalMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);

            Mathematics.MultiplyMatrices4(PreviousMatrix, resultMatrix, GlobalMatrix);
        }
        else if (resultMatrix is null && PreviousMatrix is not null)
        {
            GlobalMatrix = PreviousMatrix;
        }
        else if (resultMatrix is not null && PreviousMatrix is null)
        {
            GlobalMatrix = resultMatrix;
        }
    }

    public void Dispose()
    {
        if (_translationMatrix is not null)
            Marshal.FreeHGlobal((nint)_translationMatrix);

        if (_rotationMatrix is not null)
            Marshal.FreeHGlobal((nint)_rotationMatrix);

        if (_scaleMatrix is not null)
            Marshal.FreeHGlobal((nint)_scaleMatrix);

        if (LocalMatrix is not null)
            Marshal.FreeHGlobal((nint)LocalMatrix);

        if (GlobalMatrix is not null)
        {
            Marshal.FreeHGlobal((nint)GlobalMatrix);
            GlobalMatrix = null;
        }
    }
}
