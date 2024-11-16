using Amethyst_game_engine.Core;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;
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

    private GLBufferInfo[] _glBuffers = [];
    private int[] _glTextures = [];

    private static readonly Dictionary<string, int> _countOfComponents = new()
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

        //Task.Factory.StartNew(ReadChunkObjects);
        //Task.Factory.StartNew(() => ReadBinaryChunk(reader));

        ReadFile(reader);
        _jsonChunk = ReadJsonChunk(reader);

        ReadBinaryChunk(reader);
        ReadChunkObjects();
        ReadAccessors();
        ReadTextures();

        var t1 = Task.Factory.StartNew(ReadMetadata);
        //var t2 = Task.Factory.StartNew(ReadMultiScene);

        Task.WaitAll(t1);

        _metadata = t1.Result;
        _multiScene = ReadMultiScene();
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
        lock (_nodes) { }

        List<GLBModel> models = [];

        for (int i = 0; i < sceneNodesIndices.Length; i++)
        {
            ReadSceneRec(sceneNodesIndices[i]);
        }

        void ReadSceneRec(int nodeIndex, float[,]? previousNodeMatrix = default)
        {
            var currentNodeInfo = new NodeInfo(_nodes[nodeIndex], nodeIndex);
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

            for (int i = 0; i < node.Children!.Length; i++)
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
        lock (_meshes) { }

        Dictionary<string, object>[] primitives = ((object[])_meshes[index]["primitives"]).Cast<Dictionary<string, object>>().ToArray();
        var primitivesObj = new Primitive[primitives.Length];

        List<int> buffers = [];

        lock (_glBuffers) { }

        for (int i = 0; i < primitives.Length; i++)
        {
            var vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            var attributes = (Dictionary<string, object>)primitives[i]["attributes"];

            var currentGlBuffer = _glBuffers[(int)attributes["POSITION"]];
            var count = currentGlBuffer.count;

            GL.BindBuffer(BufferTarget.ArrayBuffer, currentGlBuffer.buffer);
            GL.VertexAttribPointer(0, 3, (VertexAttribPointerType)currentGlBuffer.componentType,
                                         currentGlBuffer.normalized,
                                         currentGlBuffer.stride, 0);

            GL.EnableVertexAttribArray(0);
            buffers.Add(currentGlBuffer.buffer);

            var currentGlBuffer2 = _glBuffers[(int)attributes["TEXCOORD_0"]];

            GL.BindBuffer(BufferTarget.ArrayBuffer, currentGlBuffer2.buffer);
            GL.VertexAttribPointer(1, 2, (VertexAttribPointerType)currentGlBuffer2.componentType,
                                         currentGlBuffer2.normalized,
                                         currentGlBuffer2.stride, 0);

            GL.EnableVertexAttribArray(1);
            buffers.Add(currentGlBuffer.buffer);

            var isIndexedGeometry = false;
            var mode = primitives[i].TryGetValue("mode", out object? modeRes) ? (int)modeRes : 4;

            if (primitives[i].TryGetValue("indices", out object? indicesRes))
            {
                isIndexedGeometry = true;
                currentGlBuffer = _glBuffers[(int)indicesRes];
                count = currentGlBuffer.count;

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, currentGlBuffer.buffer);
                buffers.Add(currentGlBuffer.buffer);
            }

            var material = (int)primitives[i]["material"];
            primitivesObj[i] = new Primitive(vertexArrayObject, currentGlBuffer.componentType, count, mode, ReadMaterial(material), isIndexedGeometry);
        }

        return new Mesh(primitivesObj, [..buffers], matrix);
    }

    private GLBufferInfo ReadAccessor(int index)
    {
        var accessor = _accessors[index];
        var accessorOffset = accessor.TryGetValue("byteOffset", out object? aOffset) ? (int)aOffset : 0;
        var normalized = accessor.TryGetValue("normalized", out object? aNormalized) && (bool)aNormalized;
        var count = (int)accessor["count"];

        var glBuffer = GL.GenBuffer();
        var type = (string)accessor["type"];
        var componentType = (int)accessor["componentType"];

        var bufferView = _bufferViews[(int)accessor["bufferView"]];
        var byteLength = (int)bufferView["byteLength"];
        var bufferViewOffset = bufferView.TryGetValue("byteOffset", out object? bOffset) ? (int)bOffset : 0;

        var typeLength = _countOfComponents[type] * _componentSize[componentType];
        var stride = bufferView.TryGetValue("byteStride", out object? byteStride) ? (int)byteStride : typeLength;
        int target = bufferView.TryGetValue("target", out object? targetResult) ? (int)targetResult : 34962;

        var bufferSlice = _binChunk[(accessorOffset + bufferViewOffset)..(bufferViewOffset + byteLength)];

        GL.BindBuffer((BufferTarget)target, glBuffer);

        GL.BufferData((BufferTarget)target,
                       bufferSlice.Length, bufferSlice,
                       BufferUsageHint.DynamicDraw);

        return new GLBufferInfo(glBuffer, stride, componentType, count, normalized);
    }

    private int ReadMaterial(int index)
    {
        var material = _materials[index];
        var attrib = (Dictionary<string, object>)material["pbrMetallicRoughness"];
        var texture = (Dictionary<string, object>)attrib["baseColorTexture"];

        return _glTextures[(int)texture["index"]];
    }

    private void ReadTextures()
    {
        lock (_textures) { }
        lock (_samplers) { }
        lock (_images) { }
        lock (_bufferViews) { }
        lock (_binChunk) { }

        lock (_glTextures)
        {
            _glTextures = new int[_textures.Length];

            for (int i = 0; i < _glTextures.Length; i++)
            {
                var handle = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, handle);

                var source = _images[(int)_textures[i]["source"]];
                var sampler = _samplers[(int)_textures[i]["sampler"]];

                var mimeType = (string)source["mimeType"] == "image/png" ? 1 : 0;
                var bufferView = _bufferViews[(int)source["bufferView"]];

                var length = (int)bufferView["byteLength"];
                var offset = bufferView.TryGetValue("byteOffset", out object? offsetRes) ? (int)offsetRes : 0;

                byte[] data = _binChunk[offset..(offset + length)];

                var components = (ColorComponents)(3 + mimeType);
                var pixelInternalFormat = (PixelInternalFormat)(6407 + mimeType);
                var pixelFormat = (PixelFormat)(6407 + mimeType);

                var wrapS = sampler.TryGetValue("wrapS", out object? wrapSRes) ? (int)wrapSRes : 10497;
                var wrapT = sampler.TryGetValue("wrapT", out object? wrapTRes) ? (int)wrapTRes : 10497;
                var magFilter = sampler.TryGetValue("magFilter", out object? magFilterRes) ? (int)magFilterRes : 9729;
                var minFilter = sampler.TryGetValue("minFilter", out object? minFilterRes) ? (int)minFilterRes : 9984;

                //if ((minFilter >> 8) == 0b_00100111)
                //    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                ImageResult image = ImageResult.FromMemory(data, components);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, wrapS);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, wrapT);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, magFilter);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, 9729);

                GL.TexImage2D(TextureTarget.Texture2D, 0,
                              pixelInternalFormat,
                              image.Width, image.Height, 0,
                              pixelFormat, PixelType.UnsignedByte,
                              image.Data);

                _glTextures[i] = handle;
            }
        }
    }

    private void ReadAccessors()
    {
        lock (_accessors) { }
        lock (_bufferViews) { }

        lock (_glBuffers)
        {
            _glBuffers = new GLBufferInfo[_accessors.Length];

            for (int i = 0; i < _accessors.Length; i++)
            {
                _glBuffers[i] = ReadAccessor(i);
            }
        }
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

        if ((readerPos & 3) != 0)
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
            _binChunk = reader.ReadBytes((int)chunkLength);
            reader.Dispose();
        }
        else
            throw new FileLoadException("Error. GLB-file is invalid");
    }
}
