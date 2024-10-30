using Amethyst_game_engine.Core;

namespace Amethyst_game_engine.Models.GLBModule;

internal readonly struct NodeInfo
{
    public string? Name { get; }
    public int[]? Children { get; }
    public int? Camera { get; }
    public int? Mesh { get; }
    public int? Skin { get; }
    public float[]? Scale { get; }
    public float[]? Translation { get; }
    public float[]? Rotation { get; }
    public float[,]? Matrix { get; }
    public float[,]? ResultMatrix { get; }

    public NodeInfo(Dictionary<string, object> nodePresentation)
    {
        if (nodePresentation.TryGetValue("name", out object? name))
            Name = (string)name;

        if (nodePresentation.TryGetValue("children", out object? children))
            Children = ((object[])children).Cast<int>().ToArray();

        if (nodePresentation.TryGetValue("camera", out object? camera))
            Camera = (int)camera;

        if (nodePresentation.TryGetValue("mesh", out object? mesh))
            Mesh = (int)mesh;

        if (nodePresentation.TryGetValue("skin", out object? skin))
            Skin = (int)skin;

        if (nodePresentation.TryGetValue("matrix", out object? matrix))
        {
            Matrix = Mathematics.GetMatrixFromArray(((object[])matrix).Cast<float>().ToArray());
            ResultMatrix = Matrix;
            return;
        }

        if (nodePresentation.TryGetValue("translation", out object? translation))
        {
            Translation = ((object[])translation).Cast<float>().ToArray();
            ResultMatrix = Mathematics.CreateTranslationMatrix(Translation[0], Translation[1], Translation[2]);
        }

        if (nodePresentation.TryGetValue("rotation", out object? rotation))
        {
            Rotation = ((object[])rotation).Cast<float>().ToArray();

            var rotationMatrix = Mathematics.ConvertQuaternionToMatrix
                (
                    Rotation[0], Rotation[1], Rotation[2], Rotation[3]
                );

            if (ResultMatrix is not null)
                ResultMatrix = Mathematics.MultiplyMatrices(ResultMatrix, rotationMatrix);
            else
                ResultMatrix = rotationMatrix;
        }

        if (nodePresentation.TryGetValue("scale", out object? scale))
        {
            Scale = ((object[])scale).Cast<float>().ToArray();

            var scaleMatrix = Mathematics.CreateScaleMatrix
                (
                    Scale[0], Scale[1], Scale[2]
                );

            if (ResultMatrix is not null)
                ResultMatrix = Mathematics.MultiplyMatrices(ResultMatrix, scaleMatrix);
            else
                ResultMatrix = scaleMatrix;
        }
    }
}
