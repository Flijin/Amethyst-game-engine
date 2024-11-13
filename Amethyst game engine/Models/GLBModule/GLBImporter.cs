using Amethyst_game_engine.Core;
using System.Numerics;
namespace Amethyst_game_engine.Models.GLBModule;

public class GLBImporter
{
    private const float MAX_SUPPORTED_VERSION = 2.0f;

    private readonly Dictionary<string, object> _jsonChunk;
    private byte[] _binChunk = [];

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

        ReadFile(reader);
        _jsonChunk = ReadJsonChunk(reader);

        Task.Factory.StartNew(ReadChunkObjects);
        Task.Factory.StartNew(() => ReadBinaryChunk(reader));

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
            int indexOfDefaultScene = 0;

            if (_jsonChunk.TryGetValue("scene", out object? res))
                indexOfDefaultScene = (int)res;

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
        lock (_nodes) { };

        List<GLBModel> models = [];

        for (int i = 0; i < sceneNodesIndices.Length; i++)
        {
            ReadSceneRec(sceneNodesIndices[i]);
        }

        void ReadSceneRec(int nodeIndex, float[,]? previousNodeMatrix = default)
        {
            var currentNodeInfo = new NodeInfo(_nodes![nodeIndex], nodeIndex);
            float[,]? globalNodeMatrix = null;

            CalculateGlobalMatrix(previousNodeMatrix, currentNodeInfo.LocalMatrix, ref globalNodeMatrix);

            if (currentNodeInfo.Mesh is not null)
            {
                models.Add(ReadModel(new NodeInfo(_nodes[nodeIndex], nodeIndex), globalNodeMatrix));
            }
            else if (currentNodeInfo.Name == "RootNode")
            {
                for (int i = 0; i < currentNodeInfo.Children!.Length; i++)
                {
                    var index = currentNodeInfo.Children[i];
                    models.Add(ReadModel(new NodeInfo(_nodes[index], index), globalNodeMatrix));
                }
            }
            else if (currentNodeInfo.Children?.Length > 1)
            {
                for (int i = 0; i < currentNodeInfo.Children.Length; i++)
                {
                    var index = currentNodeInfo.Children[i];
                    models.Add(ReadModel(new NodeInfo(_nodes[index], index), globalNodeMatrix));
                }
            }
            else if (currentNodeInfo.Children?.Length == 1)
            {
                ReadSceneRec(currentNodeInfo.Children[0], globalNodeMatrix);
            }
        }

        void CalculateGlobalMatrix(float[,]? m1, float[,]? m2, ref float[,]? result)
        {
            if (m1 is null && m2 is not null)
                result = m2;
            else if (m1 is not null && m2 is null)
                result = m1;
            else if (m1 is not null && m2 is not null)
                result = Mathematics.MultiplyMatrices(m1, m2);
            else
                result = null;
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
                meshes.Add(ReadMesh((int)node.Mesh, globalMatrix));
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

    private Mesh ReadMesh(int index, float[,]? matrix)
    {
        lock (_meshes) { };

        var primitives = ((object[])_meshes[index]["primitives"]).Cast<Dictionary<string, object>>().ToArray();
        var primitivesData = new Primitive[primitives.Length];

        lock (_accessors) { };

        for (int i = 0; i < primitives.Length; i++)
        {
            var attributes = (Dictionary<string, object>)primitives[i]["attributes"];
            var position = (int)attributes["POSITION"];
            primitivesData[i] =  new Primitive(ReadAccessor<float>(position).data);
        }

        var result = new Mesh(primitivesData);

        if (matrix is not null)
            result.Matrix = matrix;

        return result;
    }

    private int ReadAccessor(int index)
    {
        lock (_accessors) { };

        var accessor = _accessors[index];

        if (typeof(T) is float)
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

    private static void ReadFile(BinaryReader reader)
    {
        if (reader.ReadUInt32() != 0x46546C67)
            throw new FileLoadException("Error. GLB-file is invalid");

        var version = reader.ReadUInt32();

        if (version != 2)
            throw new FileLoadException($"Error. Unsupported GLB-file version: ({version}). Only version 2.x is currently supported");

        reader.ReadUInt32();
    }

    private static Dictionary<string, object> ReadJsonChunk(BinaryReader reader)
    {
        uint chunkLength;

        if ((chunkLength = reader.ReadUInt32()) > int.MaxValue)
            throw new FileLoadException("Error. GLB-file is too big");

        Dictionary<string, object?> chunkData;

        if (reader.ReadUInt32() == 0x4E4F534A)
            chunkData = JSONSerializer.JsonToObj(reader.ReadBytes((int)chunkLength));
        else
            throw new FileLoadException("Error. GLB-file is invalid");

        var readerPos = reader.BaseStream.Position;

        if (readerPos % 4 != 0)
            reader.BaseStream.Position += 4 - (readerPos % 4);

        if (chunkData.ContainsValue(null) == false)
            return chunkData!;
        else
            throw new FileLoadException("Error. GLB-file is invalid");
    }

    private void ReadBinaryChunk(BinaryReader reader)
    {
        if (reader.BaseStream.Position + 1 == reader.BaseStream.Length)
        {
            reader.Dispose();
            return;
        }

        uint chunkLength;

        if ((chunkLength = reader.ReadUInt32()) > int.MaxValue)
            throw new FileLoadException("Error. GLB-file is too big");

        if (reader.ReadUInt32() == 0x004E4942)
        {
            lock (_binChunk) _binChunk = reader.ReadBytes((int)chunkLength);
            reader.Dispose();
        }
        else
            throw new FileLoadException("Error. GLB-file is invalid");
    }
}
