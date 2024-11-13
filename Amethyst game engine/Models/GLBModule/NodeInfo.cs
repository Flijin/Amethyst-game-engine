using Amethyst_game_engine.Core;

namespace Amethyst_game_engine.Models.GLBModule;

internal struct NodeInfo
{
    private float[,]? _translationMatrix;
    private float[,]? _rotationMatrix;
    private float[,]? _scaleMatrix;
    private readonly int _index;

    public readonly int Index => _index;

    public string? Name { get; }
    public int? Mesh { get; }
    public int? Skin { get; }
    public int[]? Children { get; set; }
    public float[,]? LocalMatrix { get; private set; }
    public float[,]? GlobalMatrix { get; private set; }
    private float[,]? PreviousMatrix { get; set; }

    public NodeInfo(Dictionary<string, object> nodePresentation, int index)
    {
        _index = index;

        if (nodePresentation.TryGetValue("name", out object? name))
            Name = (string)name;

        if (nodePresentation.TryGetValue("children", out object? children))
            Children = ((object[])children).Cast<int>().ToArray();

        if (nodePresentation.TryGetValue("mesh", out object? mesh))
            Mesh = (int)mesh;

        if (nodePresentation.TryGetValue("skin", out object? skin))
            Skin = (int)skin;

        if (nodePresentation.TryGetValue("matrix", out object? matrix))
        {
            LocalMatrix = Mathematics.CreateMatrixFromArray(((object[])matrix).Cast<float>().ToArray(), true);
            return;
        }

        if (nodePresentation.TryGetValue("translation", out object? translation))
        {
            var t = ((object[])translation).Cast<float>().ToArray();

            _translationMatrix = Mathematics.CreateTranslationMatrix(t[0], t[1], t[2]);
            LocalMatrix = _translationMatrix;
        }

        if (nodePresentation.TryGetValue("rotation", out object? rotation))
        {
            var r = ((object[])rotation).Cast<float>().ToArray();

            _rotationMatrix = Mathematics.ConvertQuaternionToMatrix
            (
                r[0], r[1], r[2], r[3]
            );

            if (LocalMatrix is not null)
                LocalMatrix = Mathematics.MultiplyMatrices(LocalMatrix, _rotationMatrix);
            else
                LocalMatrix = _rotationMatrix;
        }

        if (nodePresentation.TryGetValue("scale", out object? scale))
        {
            var s = ((object[])scale).Cast<float>().ToArray();

            _scaleMatrix = Mathematics.CreateScaleMatrix
            (
                s[0], s[1], s[2]
            );

            if (LocalMatrix is not null)
                LocalMatrix = Mathematics.MultiplyMatrices(LocalMatrix, _scaleMatrix);
            else
                LocalMatrix = _scaleMatrix;
        }
    }

    public void CalculateGlobalMatrix(float[,]? matrix)
    {
        PreviousMatrix = matrix;

        if (matrix is not null && LocalMatrix is not null)
            GlobalMatrix = Mathematics.MultiplyMatrices(matrix, LocalMatrix);
        else if (matrix is not null && LocalMatrix is null)
            GlobalMatrix = matrix;
        else
            GlobalMatrix = LocalMatrix;
    }

    public void CalculateTRS(float[]? t, float[]? r, float[]? s)
    {
        float[,]? resultMatrix = null;

        if (t is not null)
            _translationMatrix = Mathematics.CreateTranslationMatrix(t[0], t[1], t[2]);
        if (r is not null)
            _rotationMatrix = Mathematics.ConvertQuaternionToMatrix(r[0], r[1], r[2], r[3]);
        if (s is not null)
            _scaleMatrix = Mathematics.CreateScaleMatrix(s[0], s[1], s[2]);

        if (_translationMatrix is not null)
        {
            resultMatrix = _translationMatrix;
        }
        if (_rotationMatrix is not null)
        {
            if (resultMatrix is not null)
                resultMatrix = Mathematics.MultiplyMatrices(resultMatrix, _rotationMatrix);
            else
                resultMatrix = _rotationMatrix;
        }
        if (_scaleMatrix is not null)
        {
            if (resultMatrix is not null)
                resultMatrix = Mathematics.MultiplyMatrices(resultMatrix, _scaleMatrix);
            else
                resultMatrix = _scaleMatrix;
        }

        LocalMatrix = resultMatrix;

        if (resultMatrix is not null && PreviousMatrix is not null)
            GlobalMatrix = Mathematics.MultiplyMatrices(PreviousMatrix, resultMatrix);
        else if (resultMatrix is null && PreviousMatrix is not null)
            GlobalMatrix = PreviousMatrix;
        else if (resultMatrix is not null && PreviousMatrix is null)
            GlobalMatrix = resultMatrix;
        else
            GlobalMatrix = null;
    }
}
