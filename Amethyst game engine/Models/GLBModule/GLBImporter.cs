using Amethyst_game_engine.Core;
using System.Numerics;
namespace Amethyst_game_engine.Models.GLBModule;

public class GLBImporter
{
    private readonly Dictionary<MetadataTypes, string> _metadata;
    private readonly GLBMultiScene _multiScene;
    private readonly Dictionary<string, object> _jsonChunk;
    private readonly BinaryReader _reader;
    private long _bufferStartPosition;

    private static readonly Dictionary<string, int> _typeCapacity = new()
    {
        { "SCALAR", 1 },
        { "VEC2",   2 },
        { "VEC3",   3 },
        { "VEC4",   4 },
        { "MAT2",   4 },
        { "MAT3",   9 },
        { "MAT4",  16 }
    };

    private static readonly Dictionary<int, int> _componentSize = new()
    {
        { 5120, 1 },
        { 5121, 1 },
        { 5122, 2 },
        { 5123, 2 },
        { 5125, 4 },
        { 5126, 4 }
    };

    public uint FileSize { get; private set; }

    public GLBImporter(string path)
    {
        _reader = new BinaryReader(new FileStream(path, FileMode.Open));

        _jsonChunk = ReadJsonChunk();
        _metadata = ReadMetadata();
        _multiScene = ReadMultiScene();
    }

    public GLBMultiScene GetMultiScene() => _multiScene;
    public GLBScene GetScene() => _multiScene.GetDefaultScene();

    public GLBModel? GetModel()
    {
        GLBScene scene = GetScene();

        if (scene.ModelsCount == 0)
            return null;
        else
            return scene.GetModelByIndex(0);
    }

    public string GetMetadata(MetadataTypes type)
    {
        if (_metadata.TryGetValue(type, out string? value) && value is string result)
            return result;
        else
            return "None";
    }

    private GLBMultiScene ReadMultiScene()
    {
        try
        {
            var indexOfDefaultScene = (int)_jsonChunk["scene"];
            var scenesObj = (object[])_jsonChunk["scenes"];
            var scenes = scenesObj.Cast<Dictionary<string, object>>().ToArray();

            if (scenesObj.Length == 0)
                throw new Exception();

            return new(ReadScenes(scenes), indexOfDefaultScene);
        }
        catch (Exception)
        {
            throw new FileLoadException("Error. GLB-file is invalid");
        }
    }

    private GLBScene[] ReadScenes(Dictionary<string, object>[] scenesElements)
    {
        GLBScene[] result;

        result = new GLBScene[scenesElements.Length];

        for (int i = 0; i < scenesElements.Length; i++)
        {
            var nodesObj = (object[])scenesElements[i]["nodes"];
            var nodes = nodesObj.Cast<int>().ToArray();

            if (scenesElements[i].TryGetValue("name", out object? name))
                result[i] = ReadScene(nodes, (string)name);
            else
                result[i] = ReadScene(nodes);
        }

        return result;
    }

    private GLBScene ReadScene(int[] sceneNodesIndices, string? name = default)
    {
        var nodes = ((object[])_jsonChunk["nodes"]).Cast<Dictionary<string, object>>().ToArray();

        List<GLBModel> models = [];

        for (int i = 0; i < sceneNodesIndices.Length; i++)
        {
            ReadSceneRec(sceneNodesIndices[i]);
        }

        void ReadSceneRec(int nodeIndex, float[,]? sceneMatrix = default)
        {
            var currentNodeInfo = new NodeInfo(nodes[nodeIndex]);
            float[,]? nodeGlobalMatrix = null;
            
            CalculateGlobalMatrix(sceneMatrix, currentNodeInfo.ResultMatrix, ref nodeGlobalMatrix);

            if (currentNodeInfo.Mesh is not null)
            {
                models.Add(ReadModel(nodes, nodes[nodeIndex], nodeGlobalMatrix));
            }
            else if (currentNodeInfo.Children?.Length > 1)
            {
                for (int j = 0; j < currentNodeInfo.Children.Length; j++)
                {
                    models.Add(ReadModel(nodes, nodes[currentNodeInfo.Children[j]], nodeGlobalMatrix));
                }
            }
            else if (currentNodeInfo.Children?.Length == 1)
            {
                ReadSceneRec(currentNodeInfo.Children[0], nodeGlobalMatrix);
            }
        }

        void CalculateGlobalMatrix(float[,]? m1, float[,]? m2, ref float[,]? result)
        {
            if (m1 is null && m2 is not null)
                result = m2;
            else if (m1 is not null && m2 is null)
                result = m1;
            else if (m1 is not null && m2 is not null)
            {
                result = new float[4, 4];
                Mathematics.MultiplyMatrices(m1, m2, result);
            }
        }

        return new GLBScene([.. models]) { Name = name ?? "None" };
    }

    private static GLBModel ReadModel(Dictionary<string, object>[] nodes, Dictionary<string, object> obj, float[,]? globalMatrix = default)
    {
        GLBModel model = new();

        return null;
    }

    private (T[] data, string layout) ReadAccessor<T>(Dictionary<string, object> accessor, Dictionary<string, object>[] bufferViews)
        where T : INumber<T>
    {
        var offset = 0;
        var stride = 0;

        var bufferView = bufferViews[(int)accessor["bufferView"]];
        var type = (string)accessor["type"];
        var typeCapacity = _typeCapacity[type];
        var componentType = (int)accessor["componentType"];
        var componentSize = _componentSize[componentType];
        var count = (int)accessor["count"];

        if (accessor.TryGetValue("byteOffset", out object? offset1))
            offset += (int)offset1;

        if (bufferView.TryGetValue("byteOffset", out object? offset2))
            offset += (int)offset2;

        if (bufferView.TryGetValue("byteStride", out object? bStride))
            stride = (int)bStride - typeCapacity * componentSize;

        var data = new T[count * typeCapacity];
        _reader.BaseStream.Position = _bufferStartPosition + offset;

        var ReadFunc = GetReaderMethod(componentType);

        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < typeCapacity; j++)
            {
                data[i] = ReadFunc();
            }
            _reader.BaseStream.Position += stride;
        }

        Func<T> GetReaderMethod(int codeType) => codeType switch
        {
            5120 => () => T.CreateChecked(_reader.ReadSByte()),
            5121 => () => T.CreateChecked(_reader.ReadByte()),
            5122 => () => T.CreateChecked(_reader.ReadInt16()),
            5123 => () => T.CreateChecked(_reader.ReadUInt16()),
            5125 => () => T.CreateChecked(_reader.ReadUInt32()),
            5126 => () => T.CreateChecked(_reader.ReadSingle()),

            _ => throw new Exception()
        };

        return (data, type);
    }

    private Dictionary<string, object> ReadJsonChunk()
    {
        if (new string(_reader.ReadChars(4)) != "glTF")
            throw new FileLoadException("Error. GLB-file is invalid");

        var version = _reader.ReadUInt32();

        if (version != 2)
            throw new FileLoadException($"Error. Unsupported GLB-file version: ({version}). Only version 2.0 is currently supported");

        FileSize = _reader.ReadUInt32();
        var chunkLength = _reader.ReadUInt32();
        Dictionary<string, object?> chunkData;

        if (new string(_reader.ReadChars(4)) == "JSON")
            chunkData = JSONSerializer.JsonToObj(_reader.ReadBytes((int)chunkLength));
        else
            throw new FileLoadException("Error. GLB-file is invalid");

        _bufferStartPosition = _reader.BaseStream.Position;

        if (chunkData.ContainsValue(null) == false)
            return chunkData!;
        else
            throw new FileLoadException("Error. GLB-file is invalid");
    }

    private Dictionary<MetadataTypes, string> ReadMetadata()
    {
        Dictionary<MetadataTypes, string> result = [];

        if ((_jsonChunk.TryGetValue("asset", out object? asset) && asset is Dictionary<string, object?> assetD) == false)
            throw new FileLoadException("Error. GLB-file is invalid");

        if ((assetD.TryGetValue("version", out object? version) && version is string versionS && versionS == "2.0") == false)
            throw new FileLoadException("Error. GLB-file is invalid");

        AddValue(assetD, MetadataTypes.Generator);

        if (assetD.TryGetValue("extras", out object? extras) && extras is Dictionary<string, object?> extrasD)
        {
            AddValue(extrasD, MetadataTypes.Author);
            AddValue(extrasD, MetadataTypes.License);
            AddValue(extrasD, MetadataTypes.Source);
            AddValue(extrasD, MetadataTypes.Title);
        }

        void AddValue(Dictionary<string, object?> from, MetadataTypes type)
        {
            if (from.TryGetValue(type.ToString().ToLower(), out object? value) && value is string valueS)
                result.Add(type, valueS);
        }

        return result;
    }
}
