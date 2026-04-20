using System.Runtime.InteropServices;
using System.Text.Json;
using Amethyst_game_engine.Core.Utilities;
using Amethyst_game_engine.Models.Components;

namespace Amethyst_game_engine.Models.GLBModule;

internal class Node
{
    private readonly bool _containsMatrixProperty = false;

    private readonly float[]? _translation;
    private readonly float[]? _scale;
    private readonly Quaternion? _rotation;

    public Node[]? Children { get; }
    public MeshData? Mesh { get; }
    public float[]? LocalMatrix { get; private set; }
    public int NodeIndex { get; }

    public Node(JsonElement rootNode, JsonElement[] nodes, MeshData[] meshes, int nodeIndex, int nestingLevel = 0)
    {
        if (nestingLevel > 500)
            throw new Exception();

        NodeIndex = nodeIndex;

        if (rootNode.TryGetProperty("translation", out JsonElement translation))
            _translation = translation.Deserialize<float[]>()!;

        if (rootNode.TryGetProperty("scale", out JsonElement scale))
            _scale = scale.Deserialize<float[]>()!;

        if (rootNode.TryGetProperty("rotation", out JsonElement rotation))
            _rotation = new(rotation.Deserialize<float[]>()!);

        if (rootNode.TryGetProperty("mesh", out JsonElement meshIndex))
            Mesh = meshes[meshIndex.GetInt32()];

        if (rootNode.TryGetProperty("matrix", out JsonElement matrix))
        {
            LocalMatrix = matrix.Deserialize<float[]>()!;
            Mathematics.TransposeMatrix4(LocalMatrix);

            _containsMatrixProperty = true;
        }

        if (rootNode.TryGetProperty("children", out JsonElement children))
        {
            Children = new Node[children.GetArrayLength()];

            var childIndex = 0;

            foreach (var child in children.EnumerateArray())
            {
                var childIndexInArray = child.GetInt32();
                Children[childIndex] = new(nodes[childIndexInArray], nodes, meshes, childIndexInArray, nestingLevel + 1);
                childIndex++;
            }
        }

        CalculateLocalMatrix();
    }

    public unsafe void CalculateLocalMatrix()
    {
        if (_containsMatrixProperty)
            return;

        var dirty = false;

        if (_scale is not null)
        {
            LocalMatrix = Mathematics.CreateScaleMatrix4(_scale[0], _scale[1], _scale[2]);
            dirty = true;
        }
        
        if (_rotation is not null)
        {
            LocalMatrix ??= new float[16];

            fixed (float* localMatrixPtr = LocalMatrix)
            {
                if (dirty == true)
                {
                    float* rotationMatrix = stackalloc float[16];
                    _rotation.Value.GetRotationMatrix(rotationMatrix);

                    float* localMatrixCopy = stackalloc float[16];
                    Buffer.MemoryCopy(localMatrixPtr, localMatrixCopy, sizeof(float) * 16, sizeof(float) * 16);

                    Mathematics.MultiplyMatrices4(localMatrixCopy, rotationMatrix, localMatrixPtr);
                }
                else
                {
                    _rotation.Value.GetRotationMatrix(localMatrixPtr);
                    dirty = true;
                }
            }
        }

        if (_translation is not null)
        {
            if (dirty == true)
            {
                fixed (float* localMatrixPtr = LocalMatrix)
                {
                    float* translationMatrix = stackalloc float[16];
                    Mathematics.CreateTranslationMatrix4(_translation[0], _translation[1], _translation[2], translationMatrix);

                    float* localMatrixCopy = stackalloc float[16];
                    Buffer.MemoryCopy(localMatrixPtr, localMatrixCopy, sizeof(float) * 16, sizeof(float) * 16);

                    Mathematics.MultiplyMatrices4(localMatrixCopy, translationMatrix, localMatrixPtr);
                }
            }
            else
            {
                LocalMatrix = _translation;
                dirty = true;
            }
        }
    }

    public List<MeshData> ExtractMeshes()
    {
        List<MeshData> result = [];

        ExtractMeshes(result, this);

        static void ExtractMeshes(List<MeshData> meshes, Node currentNode)
        {
            if (currentNode.Mesh is not null)
                meshes.Add(currentNode.Mesh);

            if (currentNode.Children != null)
            {
                foreach (var child in currentNode.Children)
                {
                    ExtractMeshes(meshes, child);
                }
            }

            return;
        }

        return result;
    }

    public unsafe void CalculateGlobalMatrices()
    {
        CalculateGlobalMatrices(null, this);

        static void CalculateGlobalMatrices(float* matrix, Node rootNode)
        {
            float* globalMatrix = stackalloc float[16];

            if (matrix is null && rootNode.LocalMatrix is not null)
            {
                fixed (float* matrixPtr = rootNode.LocalMatrix)
                    Buffer.MemoryCopy(matrixPtr, globalMatrix, sizeof(float) * 16, sizeof(float) * 16);
            }
            else if (matrix is not null && rootNode.LocalMatrix is null)
            {
                globalMatrix = matrix;
            }
            else if (matrix is not null && rootNode.LocalMatrix is not null)
            {
                fixed (float* matrixPtr = rootNode.LocalMatrix)
                    Mathematics.MultiplyMatrices4(matrix, matrixPtr, globalMatrix);
            }
            else
            {
                globalMatrix = null;
            }

            if (rootNode.Mesh is not null && globalMatrix is not null)
            {
                rootNode.Mesh.Matrix = (float*)NativeMemory.Alloc(sizeof(float) * 16);
                Buffer.MemoryCopy(globalMatrix, rootNode.Mesh.Matrix, sizeof(float) * 16, sizeof(float) * 16);
            }

            if (rootNode.Children is not null)
            {
                foreach (var child in rootNode.Children)
                {
                    CalculateGlobalMatrices(globalMatrix, child);
                }
            }
        }
    }
}
