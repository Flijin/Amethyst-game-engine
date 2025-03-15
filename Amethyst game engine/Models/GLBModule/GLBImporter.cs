using Amethyst_game_engine.Core;
using OpenTK.Graphics.OpenGL;
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

    private string[] _extensions = [];
    private string[] _extensionsRequired = [];

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

    private static readonly Dictionary<string, bool> _supportedExtensions = new()
    {
        { "KHR_materials_specular", false },
        { "KHR_materials_emissive_strength", false },
        { "KHR_materials_unlit", false }
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
        CheckExtensions();
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

    private unsafe GLBScene ReadScene(int[] sceneNodesIndices, string? name = default)
    {
        lock (_nodes) { }

        List<GLBModel> models = [];

        for (int i = 0; i < sceneNodesIndices.Length; i++)
        {
            ReadSceneRec(sceneNodesIndices[i], null);
        }

        void ReadSceneRec(int nodeIndex, float* previousNodeMatrix)
        {
            var currentNodeInfo = new NodeInfo(_nodes[nodeIndex], nodeIndex);
            float* globalNodeMatrix = stackalloc float[16];

            if (previousNodeMatrix is null && currentNodeInfo.LocalMatrix is not null)
                globalNodeMatrix = currentNodeInfo.LocalMatrix;
            else if (previousNodeMatrix is not null && currentNodeInfo.LocalMatrix is null)
                globalNodeMatrix = previousNodeMatrix;
            else if (previousNodeMatrix is not null && currentNodeInfo.LocalMatrix is not null)
                Mathematics.MultiplyMatrices4(previousNodeMatrix, currentNodeInfo.LocalMatrix, globalNodeMatrix);
            else
                globalNodeMatrix = null;

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

            currentNodeInfo.Dispose();
        }

        return new GLBScene([.. models]) { Name = name ?? "None" };
    }

    private unsafe GLBModel ReadModel(NodeInfo node, float* globalMatrix)
    {
        var modelNodes = new NodeInfo[_nodes.Length];
        List<Mesh> meshes = [];

        if (ReadNodes(node))
            CalculateMatricesAndAddMeshes(modelNodes[node.Index], globalMatrix);

        return new GLBModel(modelNodes, [..meshes]) { Name = node.Name ?? "None" };

        void CalculateMatricesAndAddMeshes(NodeInfo node, float* matrix)
        {
            node.CalculateGlobalMatrix(matrix);

            for (int i = 0; i < node.Children!.Length; i++)
            {
                CalculateMatricesAndAddMeshes(modelNodes[node.Children[i]], node.GlobalMatrix);
            }

            if (node.Mesh is not null)
                meshes.Add(ReadMesh((int)node.Mesh, node.GlobalMatrix));
        }

        bool ReadNodes(NodeInfo node)
        {
            if (node.Children is null && node.Mesh is null)
            {
                node.Dispose();
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
                    node.Dispose();
                    return false;
                }
            }
        }
    }

    private unsafe Mesh ReadMesh(int index, float* matrix)
    {
        lock (_meshes) { }

        Dictionary<string, object>[] primitivesDicts = ((object[])_meshes[index]["primitives"]).Cast<Dictionary<string, object>>().ToArray();
        Primitive[] primitives = new Primitive[primitivesDicts.Length];

        List<int> buffers = [];

        for (int i = 0; i < primitivesDicts.Length; i++)
        {
            var vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            var attributes = (Dictionary<string, object>)primitivesDicts[i]["attributes"];
            var currentBuffer = ReadAccessor((int)attributes["POSITION"], BufferTarget.ArrayBuffer);

            AddAttribute(buffers, currentBuffer, 0);

            if (attributes.TryGetValue("COLOR_0", out object? color))
                AddAttribute(buffers, ReadAccessor((int)color, BufferTarget.ArrayBuffer), 1);

            if (attributes.TryGetValue("TEXCOORD_0", out object? textCoord))
                AddAttribute(buffers, ReadAccessor((int)textCoord, BufferTarget.ArrayBuffer), 2);

            var primitive = new Primitive(vertexArrayObject)
            {
                mode = (primitivesDicts[i].TryGetValue("mode", out object? modeRes) ?
                (OpenTK.Graphics.ES30.PrimitiveType)modeRes : OpenTK.Graphics.ES30.PrimitiveType.Triangles),

                Material = new()
            };

            if (primitivesDicts[i].TryGetValue("material", out object? materialIndex))
                ReadMaterial(ref primitive, buffers, (int)materialIndex);

            if (primitivesDicts[i].TryGetValue("indices", out object? indicesRes))
            {
                currentBuffer = ReadAccessor((int)indicesRes, BufferTarget.ElementArrayBuffer);
                primitive.isIndexedGeometry = true;
                primitive.componentType = currentBuffer.componentType;

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, currentBuffer.buffer);
                buffers.Add(currentBuffer.buffer);
            }

            primitive.count = currentBuffer.count;

            primitives[i] = primitive;

            GL.BindVertexArray(0);
        }

        return new Mesh(primitives, [..buffers]) { Matrix = matrix };
    }

    private unsafe GLBufferInfo ReadAccessor(int index, BufferTarget target)
    {
        var accessor = _accessors[index];
        var accessorOffset = accessor.TryGetValue("byteOffset", out object? aOffset) ? (int)aOffset : 0;

        var glBuffer = GL.GenBuffer();
        var type = (string)accessor["type"];
        var componentType = (int)accessor["componentType"];

        var bufferView = _bufferViews[(int)accessor["bufferView"]];
        var byteLength = (int)bufferView["byteLength"];
        var bufferViewOffset = bufferView.TryGetValue("byteOffset", out object? bOffset) ? (int)bOffset : 0;

        var typeLength = _countOfComponents[type] * _componentSize[componentType];
        var stride = bufferView.TryGetValue("byteStride", out object? byteStride) ? (int)byteStride : typeLength;

        var result = new GLBufferInfo(glBuffer, componentType)
        {
            count = (int)accessor["count"],
            countOfComponents = _countOfComponents[type],
            normalized = accessor.TryGetValue("normalized", out object? aNormalized) && (bool)aNormalized,
            stride = stride, 
        };

        GL.BindBuffer(target, glBuffer);

        fixed (byte* ptr = _binChunk)
        {
            GL.BufferData(target,
               byteLength - accessorOffset, ((nint)ptr) + accessorOffset + bufferViewOffset,
               BufferUsageHint.DynamicDraw);
        }

        GL.BindBuffer(target, 0);

        return result;
    }

    private void ReadMaterial(ref Primitive primitive, List<int> buffers, int materialIndex)
    {
        var material = _materials[materialIndex];
        
        if (material.TryGetValue("pbrMetallicRoughness", out object? mainMaterialObj) &&
            mainMaterialObj is Dictionary<string, object> mainMaterial)
        {
            var baseColorFactor = new float[4] { 1.0f, 1.0f, 1.0f, 1.0f };
            var metallicRoughnessFactors = new float[2] { 0.0f, 0.0f };
            var albedoHandle = -1;
            var metallicRoughnessHandle = -1;

            if (mainMaterial.TryGetValue("baseColorFactor", out object? baseColorFactorObj))
            {
                baseColorFactor = ((object[])baseColorFactorObj).Select(el => el = Convert.ToSingle(el))
                                                                .Cast<float>()
                                                                .ToArray();
            }

            if (mainMaterial.TryGetValue("metallicFactor", out object? metallicFactor))
                metallicRoughnessFactors[0] = Convert.ToSingle(metallicFactor);

            if (mainMaterial.TryGetValue("roughnessFactor", out object? roughnessFactor))
                metallicRoughnessFactors[1] = Convert.ToSingle(roughnessFactor);

            if (mainMaterial.TryGetValue("baseColorTexture", out object? baseColorTextureObj) &&
                baseColorTextureObj is Dictionary<string, object> baseColorTexture)
            {
                albedoHandle = _glTextures[(int)baseColorTexture["index"]];

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, albedoHandle);

                //if (baseColorTexture.TryGetValue("texCoord", out object? texCoordIndex))
                //    AddAttribute(buffers, ReadAccessor((int)texCoordIndex, BufferTarget.ArrayBuffer), 2);
            }

            if (mainMaterial.TryGetValue("metallicRoughnessTexture", out object? metallicRoughnessTextureObj) &&
                    metallicRoughnessTextureObj is Dictionary<string, object> metallicRoughnessTexture)
            {
                metallicRoughnessHandle = _glTextures[(int)metallicRoughnessTexture["index"]];

                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, metallicRoughnessHandle);

                //if (metallicRoughnessTexture.TryGetValue("texCoord", out object? texCoordIndex))
                //    AddAttribute(buffers, ReadAccessor((int)texCoordIndex, BufferTarget.ArrayBuffer), 2);
            }

            //primitive.material[MaterialsProperties.Albedo] = (baseColorFactor, albedoHandle);
            //primitive.material[MaterialsProperties.MetallicRoughness] = (metallicRoughnessFactors, metallicRoughnessHandle);
            
            //primitive.materialsUsed[0] = 1;
            //primitive.materialsUsed[1] = 1;

            //primitive.textureHandles[0] = albedoHandle;
            //primitive.textureHandles[1] = metallicRoughnessHandle;
        }
    }

    private static void AddAttribute(List<int> buffers, GLBufferInfo buffer, int location)
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, buffer.buffer);
        GL.VertexAttribPointer(location, buffer.countOfComponents,
                               (VertexAttribPointerType)buffer.componentType,
                               buffer.normalized,
                               buffer.stride, 0);
        
        GL.EnableVertexAttribArray(location);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        buffers.Add(buffer.buffer);
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
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, handle);

                var source = _images[(int)_textures[i]["source"]];
                var sampler = _textures[i].TryGetValue("sampler", out object? samplerRes) ? _samplers[(int)samplerRes] : [];

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

                GL.BindTexture(TextureTarget.Texture2D, 0);
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

        lock (_extensions)
        {
            _extensions = _jsonChunk.TryGetValue("extensionsUsed", out object? extensionsUsed) ?
                ((object[])extensionsUsed).Cast<string>().ToArray() : [];
        }

        lock (_extensionsRequired)
        {
            _extensionsRequired = _jsonChunk.TryGetValue("extensionsRequired", out object? extensionsRequired) ?
                ((object[])extensionsRequired).Cast<string>().ToArray() : [];
        }

        Dictionary<string, object>[] ReadObjectsFromChunk(string objName)
        {
            if (_jsonChunk.TryGetValue(objName, out object? obj))
                return ((object[])obj).Cast<Dictionary<string, object>>().ToArray();
            else
                return [];
        }
    }

    private void CheckExtensions()
    {
        foreach (var extension in _extensions)
        {
            if (_supportedExtensions.ContainsKey(extension))
                _supportedExtensions[extension] = true;
        }

        foreach (var extensionRequired in _extensionsRequired)
        {
            if (_supportedExtensions.ContainsKey(extensionRequired) == false)
                throw new FileLoadException($"Error. The GLB-file uses an {extensionRequired} extension that is not supported.");
        }
    }

    private Dictionary<MetadataTypes, string> ReadMetadata()
    {
        Dictionary<MetadataTypes, string> result = [];

        if ((_jsonChunk.TryGetValue("asset", out object? asset) && asset is Dictionary<string, object?> assetD) == false)
            throw new FileLoadException("Error. GLB-file is invalid.");

        if ((assetD.TryGetValue("version", out object? version) && version is string versionS && versionS[0] == '2') == false)
            throw new FileLoadException("Error. GLB-file is invalid.");

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
