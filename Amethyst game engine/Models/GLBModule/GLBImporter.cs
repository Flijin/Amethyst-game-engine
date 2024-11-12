using Amethyst_game_engine.Core;
using System.Numerics;
namespace Amethyst_game_engine.Models.GLBModule;

public class GLBImporter
{
    private const float MAX_SUPPORTED_VERSION = 2.0f;
    
    private uint _fileSize;
    private byte[] _buffer = [];

    private readonly Dictionary<string, object> _jsonChunk;
    private readonly Dictionary<MetadataTypes, string>? _metadata;
    private readonly GLBMultiScene _multiScene;

    private Dictionary<string, object>[] _accessors = [];
    private Dictionary<string, object>[] _bufferViews = [];
    private Dictionary<string, object>[] _images = [];
    private Dictionary<string, object>[] _materials = [];
    private Dictionary<string, object>[] _meshes = [];
    private Dictionary<string, object>[] _nodes = [];
    private Dictionary<string, object>[] _samplers = [];
    private Dictionary<string, object>[] _skins = [];
    private Dictionary<string, object>[] _textures = [];

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

    public GLBImporter(string path)
    {
        BinaryReader reader = new(new FileStream(path, FileMode.Open));
        _jsonChunk = ReadJsonChunk(reader);

        Task.Factory.StartNew(ReadChunkObjects);
        Task.Factory.StartNew(() => ReadBuffer(reader));

        var t1 = Task.Factory.StartNew(ReadMetadata);
        var t2 = Task.Factory.StartNew(ReadMultiScene);

        Task.WaitAll(t1, t2);

        _metadata = t1.Result;
        _multiScene = t2.Result;
    }

    public GLBMultiScene GetMultiScene() => _multiScene;
    public GLBScene GetScene() => _multiScene!.GetDefaultScene();

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
        if (_metadata!.TryGetValue(type, out string? value) && value is string result)
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
        List<GLBModel> models = [];

        for (int i = 0; i < sceneNodesIndices.Length; i++)
        {
            lock(_nodes) ReadSceneRec(sceneNodesIndices[i]);
        }

        void ReadSceneRec(int nodeIndex, float[,]? sceneMatrix = default)
        {
            var currentNodeInfo = new NodeInfo(_nodes![nodeIndex], nodeIndex);
            float[,]? nodeGlobalMatrix = null;

            CalculateGlobalMatrix(sceneMatrix, currentNodeInfo.LocalMatrix, ref nodeGlobalMatrix);

            if (currentNodeInfo.Mesh is not null)
            {
                models.Add(ReadModel(new NodeInfo(_nodes[nodeIndex], nodeIndex), nodeGlobalMatrix));
            }
            else if (currentNodeInfo.Name == "RootNode")
            {
                for (int i = 0; i < currentNodeInfo.Children!.Length; i++)
                {
                    var index = currentNodeInfo.Children[i];
                    models.Add(ReadModel(new NodeInfo(_nodes[index], index), nodeGlobalMatrix));
                }
            }
            else if (currentNodeInfo.Children?.Length > 1)
            {
                for (int i = 0; i < currentNodeInfo.Children.Length; i++)
                {
                    var index = currentNodeInfo.Children[i];
                    models.Add(ReadModel(new NodeInfo(_nodes[index], index), nodeGlobalMatrix));
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
            {
                result = m2;
            }
            else if (m1 is not null && m2 is null)
            {
                result = m1;
            }
            else if (m1 is not null && m2 is not null)
            {
                result = new float[4, 4];
                Mathematics.MultiplyMatrices(m1, m2, result);
            }
            else
            {
                result = null;
            }
        }

        return new GLBScene([.. models]) { Name = name ?? "None" };
    }

    private GLBModel ReadModel(NodeInfo node, float[,]? globalMatrix = default)
    {
        NodeInfo?[] modelNodes = new NodeInfo?[_nodes.Length];
        List<Mesh> meshes = [];

        if (ReadNodes(node))
            CalculateMatricesAndAddMeshes(node, globalMatrix);

        return new GLBModel(modelNodes, [..meshes]) { Name = node.Name ?? "None" };

        void CalculateMatricesAndAddMeshes(NodeInfo node, float[,]? matrix)
        {
            node.CalculateGlobalMatrix(matrix);

            for (int i = 0; i < node.Children?.Length; i++)
            {
                CalculateMatricesAndAddMeshes((NodeInfo)modelNodes[node.Children[i]]!, node.GlobalMatrix);
            }

            if (node.Mesh is not null)
                meshes.Add(ReadMesh());
        }

        bool ReadNodes(NodeInfo node)
        {
            if (node.Children is null && node.Mesh is null)
            {
                return false;
            }
            else if (node.Children is null && node.Mesh is not null)
            {
                node.Children = [];
                modelNodes[node.Index] = node;

                return true;
            }
            else
            {
                var containsMesh = node.Mesh != null;

                List<int> newChildren = [];

                for (int i = 0; i < node.Children!.Length; i++)
                {
                    var index = node.Children[i];

                    if (ReadNodes(new NodeInfo(_nodes[index], index)))
                    {
                        newChildren.Add(index);
                        containsMesh = true;
                    }
                }

                if (containsMesh)
                {
                    node.Children = [.. newChildren];
                    modelNodes[node.Index] = node;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }

    private static Mesh ReadMesh()
    {
        return new Mesh();
    }

    //private (T[] data, string layout) ReadAccessor<T>(Dictionary<string, object> accessor)
    //where T : INumber<T>
    //{
    //    var offset = 0;
    //    var stride = 0;
    //    Dictionary<string, object> bufferView;

    //    lock (_bufferViews) bufferView = _bufferViews[(int)accessor["bufferView"]];

    //    if (T is int)
    //    var type = (string)accessor["type"];
    //    var typeCapacity = _typeCapacity[type];
    //    var componentType = (int)accessor["componentType"];
    //    var componentSize = _componentSize[componentType];
    //    var count = (int)accessor["count"];

    //    if (accessor.TryGetValue("byteOffset", out object? offset1))
    //        offset += (int)offset1;

    //    if (bufferView.TryGetValue("byteOffset", out object? offset2))
    //        offset += (int)offset2;

    //    if (bufferView.TryGetValue("byteStride", out object? bStride))
    //        stride = (int)bStride - typeCapacity * componentSize;

    //    var data = new T[count * typeCapacity];
    //    _reader.BaseStream.Position = _bufferStartPosition + offset;

    //    var ReadFunc = GetReaderMethod(componentType);

    //    for (int i = 0; i < count; i++)
    //    {
    //        for (int j = 0; j < typeCapacity; j++)
    //        {
    //            data[i] = ReadFunc();
    //        }
    //        _reader.BaseStream.Position += stride;
    //    }

    //    Func<T> GetReaderMethod(int codeType) => codeType switch
    //    {
    //        5120 => () => T.CreateChecked(_reader.ReadSByte()),
    //        5121 => () => T.CreateChecked(_reader.ReadByte()),
    //        5122 => () => T.CreateChecked(_reader.ReadInt16()),
    //        5123 => () => T.CreateChecked(_reader.ReadUInt16()),
    //        5125 => () => T.CreateChecked(_reader.ReadUInt32()),
    //        5126 => () => T.CreateChecked(_reader.ReadSingle()),

    //        _ => throw new Exception()
    //    };

    //    return (data, type);
    //}

    private void ReadBuffer(BinaryReader reader)
    {
        lock (_buffer) _buffer = reader.ReadBytes((int)(_fileSize - reader.BaseStream.Position));
        reader.Dispose();
    }

    private void ReadChunkObjects()
    {
        lock (_accessors) _accessors = ReadObjectsFromChunk("accessors");
        lock (_bufferViews) _bufferViews = ReadObjectsFromChunk("bufferViews");
        lock (_images) _images = ReadObjectsFromChunk("images");
        lock (_materials) _materials = ReadObjectsFromChunk("materials");
        lock (_meshes) _meshes = ReadObjectsFromChunk("meshes");
        lock (_nodes) _nodes = ReadObjectsFromChunk("nodes");
        lock (_samplers) _samplers = ReadObjectsFromChunk("samplers");
        lock (_skins) _skins = ReadObjectsFromChunk("skins");
        lock (_textures) _textures = ReadObjectsFromChunk("textures");

        Dictionary<string, object>[] ReadObjectsFromChunk(string objName)
        {
            if (_jsonChunk.TryGetValue(objName, out object? obj))
                return ((object[])obj).Cast<Dictionary<string, object>>().ToArray();
            else
                return [];
        }
    }

    private Dictionary<MetadataTypes, string> ReadMetadata()
    {
        Dictionary<MetadataTypes, string> result = [];

        if ((_jsonChunk.TryGetValue("asset", out object? asset) && asset is Dictionary<string, object?> assetD) == false)
            throw new FileLoadException("Error. GLB-file is invalid");

        if ((assetD.TryGetValue("version", out object? version) && version is string versionS && versionS[0] == '2') == false)
            throw new FileLoadException("Error. GLB-file is invalid");

        if (assetD.TryGetValue("minVersion", out object? minVersion) &&
            minVersion is string minVersionS &&
            float.Parse(minVersionS) > MAX_SUPPORTED_VERSION)
        {
            throw new FileLoadException($"The file requires glTF {minVersionS} support. Max 2.0 is supported.");
        }

        AddValue(assetD, MetadataTypes.Generator);
        AddValue(assetD, MetadataTypes.Copyright);

        void AddValue(Dictionary<string, object?> from, MetadataTypes type)
        {
            if (from.TryGetValue(type.ToString().ToLower(), out object? value) && value is string valueS)
                result.Add(type, valueS);
        }

        return result;
    }

    private Dictionary<string, object> ReadJsonChunk(BinaryReader reader)
    {
        if (reader.ReadUInt32() != 0x46546C67)
            throw new FileLoadException("Error. GLB-file is invalid");

        var version = reader.ReadUInt32();

        if (version != 2)
            throw new FileLoadException($"Error. Unsupported GLB-file version: ({version}). Only version 2.x is currently supported");

        _fileSize = reader.ReadUInt32();
        uint chunkLength;

        if ((chunkLength = reader.ReadUInt32()) > int.MaxValue)
            throw new FileLoadException("Error. GLB-file is too big");

        Dictionary<string, object?> chunkData;

        if (reader.ReadUInt32() == 0x4E4F534A)
            chunkData = JSONSerializer.JsonToObj(reader.ReadBytes((int)chunkLength));
        else
            throw new FileLoadException("Error. GLB-file is invalid");

        if (chunkData.ContainsValue(null) == false)
            return chunkData!;
        else
            throw new FileLoadException("Error. GLB-file is invalid");
    }
}
