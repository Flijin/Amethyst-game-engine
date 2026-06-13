using Amethyst_game_engine.Core.GameObjects.Components;
using Amethyst_game_engine.Core.Render.Settings;
using Amethyst_game_engine.Core.Utilities;
using Amethyst_game_engine.Models.Components;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections;
using System.Text.Json;

namespace Amethyst_game_engine.Models.GLBModule;

public static class GLBImporter
{
    private struct GLTFComponentsData
    {
        public BufferViewData[] bufferViews;
        public AccessorData[] accessors;
        public TextureData[] textures;
        public MaterialData[] materials;
        public Sampler[] samplers;
        public Memory<byte>[] imagesData;
        public HashSet<int> joints;
        public MeshData[] meshes;
    }

    private struct AccessorData
    {
        public byte[] data;
        public int componentType;
        public bool normalized;
        public int count;
        public Vector3? min;
        public Vector3? max;
    }

    private struct BufferViewData
    {
        public Memory<byte> data;
        public int stride;
    }

    public static GLBScene[]? LoadModel(string path, RenderSettings settings = RenderSettings.All)
    {
        if (File.Exists(path) == false)
        {
            SystemCalls.PrintMessage($"Error. GLB-file {path} does not exists", MessageTypes.ErrorMessage);
            return null;
        }

        using FileStream stream = File.OpenRead(path);
        using BinaryReader reader = new(stream);

        if (stream.Length > 1024L * 1024L * 1024L)
        {
            SystemCalls.PrintMessage($"Error. GLB-file {path} is too big. Max supported size: 1 GB", MessageTypes.ErrorMessage);
            return null;
        }

        try
        {
            return ReadFile(reader, path, settings);
        }
        catch (Exception)
        {
           SystemCalls.PrintMessage($"Error. GLB-file {path} is invalid", MessageTypes.ErrorMessage);
           return null;
        }
    }

    private static GLBScene[]? ReadFile(BinaryReader reader, string path, RenderSettings settings)
    {
        var magic = reader.ReadUInt32();

        if (magic != 0x46546C67)
        {
            SystemCalls.PrintMessage($"Error. GLB-file {path} is not valid", MessageTypes.ErrorMessage);
            return null;
        }

        void PrintUnsupportedVersion(object version)
        {
            SystemCalls.PrintMessage($"Error. Unsupported GLB-file {path} version: ({version}). Only version 2.x is currently supported", MessageTypes.ErrorMessage);
        }

        var version = reader.ReadUInt32();

        if (version != 2)
        {
            PrintUnsupportedVersion(version);
            return null;
        }

        reader.BaseStream.Seek(sizeof(uint), SeekOrigin.Current);

        JsonElement? jsonChunkNullable = ReadJsonChunk(reader);
        byte[]? binChunk = ReadBinaryChunk(reader);

        if (jsonChunkNullable is null || binChunk is null)
        {
            SystemCalls.PrintMessage($"Error. GLB-file {path} is not valid", MessageTypes.ErrorMessage);
            return null;
        }

        var jsonChunk = (JsonElement)jsonChunkNullable;

        var asset = jsonChunk.GetProperty("asset");
        var versionJson = asset.GetProperty("version").GetString()!;

        if (versionJson[0] != '2')
        {
            PrintUnsupportedVersion(versionJson);
            return null;
        }

        var result = ReadScenes(jsonChunk, binChunk, settings, path);

        if (result is null)
            return null;

        return result;
    }

    private static JsonElement? ReadJsonChunk(BinaryReader reader)
    {
        JsonElement? chunkData;

        var chunkLength = reader.ReadUInt32();
        var chunkType = reader.ReadUInt32();

        if (chunkType == 0x4E4F534A)
        {
            //File.WriteAllText(@"C:\Users\it_ge\Desktop\1.txt", Encoding.UTF8.GetString(reader.ReadBytes((int)chunkLength)));
            JsonDocument doc = JsonDocument.Parse(reader.ReadBytes((int)chunkLength));
            chunkData = doc.RootElement.Clone();
            doc.Dispose();
        }
        else
        {
            return null;
        }

        var baseStream = reader.BaseStream;
        var padding = (int)((4 - (reader.BaseStream.Position % 4)) % 4);

        baseStream.Seek(padding, SeekOrigin.Current);

        return chunkData;
    }

    private static byte[]? ReadBinaryChunk(BinaryReader reader)
    {
        if (reader.BaseStream.Position == reader.BaseStream.Length)
            return [];

        var chunkLength = reader.ReadUInt32();
        var chunkType = reader.ReadUInt32();

        if (chunkType == 0x004E4942)
            return reader.ReadBytes((int)chunkLength);
        else
            return null;
    }

    private static GLBScene[]? ReadScenes(JsonElement jsonChunk, byte[] binChunk, RenderSettings settings, string path)
    {
        GLBScene[] result;

        byte[][]? buffers = ReadBuffers(jsonChunk.GetProperty("buffers"), binChunk);

        if (buffers is null)
            return null;

        GLTFComponentsData componentsData = new()
        {
            bufferViews = ReadBufferViews(jsonChunk.GetProperty("bufferViews"), buffers),
        };

        componentsData.accessors = ReadAccessors(jsonChunk.GetProperty("accessors"), componentsData.bufferViews);

        if (jsonChunk.TryGetProperty("samplers", out JsonElement samplers))
            componentsData.samplers = ReadSamplers(samplers);

        if (jsonChunk.TryGetProperty("images", out JsonElement images))
            componentsData.imagesData = ReadImages(images, componentsData.bufferViews);

        if (jsonChunk.TryGetProperty("textures", out JsonElement textures))
            componentsData.textures = ReadTextures(textures, componentsData);

        if (jsonChunk.TryGetProperty("materials", out JsonElement materials))
            componentsData.materials = ReadMaterials(materials, componentsData, settings);

        if (jsonChunk.TryGetProperty("skins", out JsonElement skins))
            componentsData.joints = ReadJoints(skins);
        else
            componentsData.joints = [];

        componentsData.meshes = ReadMeshes(jsonChunk.GetProperty("meshes"), componentsData);

        JsonElement scenesJson = jsonChunk.GetProperty("scenes");
        result = new GLBScene[scenesJson.GetArrayLength()];

        int sceneIndex = 0;

        JsonElement[] nodes = jsonChunk.GetProperty("nodes").Deserialize<JsonElement[]>()!;

        foreach (var scene in scenesJson.EnumerateArray())
        {
            string? sceneName;

            if (scene.TryGetProperty("name", out JsonElement sceneNameEl))
                sceneName = sceneNameEl.GetString();
            else
                sceneName = null;

            int[] sceneNodesIndises = scene.GetProperty("nodes").Deserialize<int[]>()!;
            Node[] sceneNodes = new Node[sceneNodesIndises.Length];

            for (int i = 0; i < sceneNodes.Length; i++)
            {
                sceneNodes[i] = new(nodes[sceneNodesIndises[i]], nodes, componentsData.meshes, sceneNodesIndises[i]);
            }

            result[sceneIndex] = ReadScene(sceneName, sceneNodes, componentsData, path);
        }

        return result;
    }

    private static GLBScene ReadScene(string? sceneName, Node[] sceneNodes, GLTFComponentsData components, string path)
    {
        List<GLBModel> sceneModels = [];

        foreach (var node in sceneNodes)
        {
            CalculateLocalMatrices(node);
            node.CalculateGlobalMatrices();
            ExtractModels(node);
        }

        static void CalculateLocalMatrices(Node node)
        {
            node.CalculateLocalMatrix();

            if (node.Children is not null)
            {
                foreach (var child in node.Children)
                {
                    CalculateLocalMatrices(child);
                }
            }
        }

        void ExtractModels(Node node)
        {
            if (components.joints.Contains(node.NodeIndex) || node.Mesh is not null)
            {
                sceneModels.Add(new GLBModel(node.ExtractMeshes())
                {
                    Path = path,
                });

                return;
            }

            if (node.Children is not null)
            {
                foreach (var child in node.Children)
                {
                    ExtractModels(child);
                }
            }
        }

        sceneModels.RemoveAll(model => model.MeshesData.Count == 0);

        for (int i = 0; i < sceneModels.Count; i++)
            sceneModels[i].GLBModelIndex = i;

        return new GLBScene(sceneModels) { Name = sceneName };
    }

    private static byte[][]? ReadBuffers(JsonElement buffers, byte[] binChunk)
    {
        byte[][]? result = new byte[buffers.GetArrayLength()][];
        int bufferIndex = 0;
        int minMediatypeLength = 22;

        foreach (var buffer in buffers.EnumerateArray())
        {
            if (buffer.TryGetProperty("uri", out JsonElement uri))
            {
                string uriString = uri.GetString()!;
                int endURI = uriString.IndexOf("base64,", minMediatypeLength, StringComparison.Ordinal);

                if (endURI == -1)
                    return null;

                string base64 = uriString[(endURI + "base64,".Length)..];
                result[bufferIndex] = Convert.FromBase64String(base64);
            }
            else
            {
                result[bufferIndex] = binChunk;
            }

            bufferIndex++;
        }

        return result;
    }

    private static BufferViewData[] ReadBufferViews(JsonElement bufferViews, byte[][] buffers)
    {
        var result = new BufferViewData[bufferViews.GetArrayLength()];
        int bufferViewIndex = 0;

        foreach (var bufferView in bufferViews.EnumerateArray())
        {
            int bufferIndex = bufferView.GetProperty("buffer").GetInt32();
            int byteLength = bufferView.GetProperty("byteLength").GetInt32();
            int byteOffset = bufferView.TryGetProperty("byteOffset", out JsonElement byteOffsetEl) ? byteOffsetEl.GetInt32() : 0;
            int byteStride = bufferView.TryGetProperty("byteStride", out JsonElement byteStrideEl) ? byteStrideEl.GetInt32() : 0;

            result[bufferViewIndex] = new()
            {
                data = new Memory<byte>(buffers[bufferIndex], byteOffset, byteLength),
                stride = byteStride
            };

            bufferViewIndex++;
        }

        return result;
    }

    private static AccessorData[] ReadAccessors(JsonElement accessors, BufferViewData[] bufferViews)
    {
        var result = new AccessorData[accessors.GetArrayLength()];
        int accessorIndex = 0;

        foreach (var accessor in accessors.EnumerateArray())
        {
            int componentType = accessor.GetProperty("componentType").GetInt32();
            int count = accessor.GetProperty("count").GetInt32();
            string type = accessor.GetProperty("type").GetString()!;

            int elementSize = Helpers.GetNumberOfComponents(type) * Helpers.SizeOfComponent(componentType);

            int byteOffset = accessor.TryGetProperty("byteOffset", out JsonElement byteOffsetEl) ? byteOffsetEl.GetInt32() : 0;
            bool normalized = accessor.TryGetProperty("normalized", out JsonElement normalizedEl) && normalizedEl.GetBoolean();
            int bufferView = accessor.TryGetProperty("bufferView", out JsonElement bufferViewEl) ? bufferViewEl.GetInt32() : -1;

            Vector3? min = null;
            Vector3? max = null;

            if (accessor.TryGetProperty("min", out JsonElement minJson))
            {
                float[] minArray = minJson.Deserialize<float[]>()!;
                min = new(minArray[0], minArray[1], minArray.Length == 3 ? minArray[2] : -1);
            }

            if (accessor.TryGetProperty("max", out JsonElement maxJson))
            {
                float[] maxArray = maxJson.Deserialize<float[]>()!;
                max = new(maxArray[0], maxArray[1], maxArray.Length == 3 ? maxArray[2] : -1);
            }

            byte[] data;

            if (bufferView == -1)
                data = new byte[count * elementSize];
            else
                data = CopyData(bufferViews[bufferView], byteOffset, elementSize, count);

            if (accessor.TryGetProperty("sparse", out JsonElement sparse))
            {
                int countSparse = sparse.GetProperty("count").GetInt32();
                JsonElement indices = sparse.GetProperty("indices");
                JsonElement values = sparse.GetProperty("values");

                int indicesOffset = indices.TryGetProperty("byteOffset", out JsonElement indicesOffsetEl) ? indicesOffsetEl.GetInt32() : 0;
                int indicesComponentType = indices.GetProperty("componentType").GetInt32();
                int indicesBufferView = indices.GetProperty("bufferView").GetInt32();

                byte[] indicesData = CopyData(bufferViews[indicesBufferView], indicesOffset, Helpers.SizeOfComponent(indicesComponentType), count);

                int valuesOffset = values.TryGetProperty("byteOffset", out JsonElement valuesOffsetEl) ? valuesOffsetEl.GetInt32() : 0;
                int valuesBufferView = values.GetProperty("bufferView").GetInt32();

                byte[] valuesData = CopyData(bufferViews[valuesBufferView], valuesOffset, elementSize, count);

                switch (indicesComponentType)
                {
                    case 5121:
                        ReplaceSparseElements<byte>(data, indicesData, valuesData, countSparse, elementSize); break;
                    case 5123:
                        ReplaceSparseElements<ushort>(data, indicesData, valuesData, countSparse, elementSize); break;
                    default:
                        ReplaceSparseElements<uint>(data, indicesData, valuesData, countSparse, elementSize); break;
                }
            }

            result[accessorIndex] = new()
            {
                data = data,
                componentType = componentType,
                normalized = normalized,
                count = count,
                min = min,
                max = max
            };

            accessorIndex++;
        }

        static byte[] CopyData(BufferViewData bufferView, int offset, int elementSize, int count)
        {
            if (bufferView.stride == 0 || bufferView.stride == elementSize)
                return bufferView.data.Slice(offset, elementSize * count).ToArray();

            byte[] result = new byte[elementSize * count];
            
            int step = bufferView.stride - elementSize;
            int srcOffset = offset;
            int dstOffset = 0;

            for (int i = 0; i < count; i++)
            {
                bufferView.data.Slice(srcOffset, elementSize).CopyTo(result.AsMemory(dstOffset));
                dstOffset += elementSize;
                srcOffset += elementSize + step;
            }

            return result;
        }

        static unsafe void ReplaceSparseElements<T>(byte[] data,
            byte[] indicesData,
            byte[] valuesData,
            int count,
            int elementSize) where T : unmanaged
        {
            fixed (byte* dataPtr = &data[0])
            fixed (byte* valuesPtr = &valuesData[0])
            fixed (void* indicesPtr = &indicesData[0])
            {
                T* indicesPtrTyped = (T*)indicesPtr;

                int indexSize = sizeof(T);
                for (int i = 0; i < count; i++)
                {
                    int currentIndex = indexSize switch
                    {
                        1 => ((byte*)indicesPtrTyped)[i],
                        2 => ((ushort*)indicesPtrTyped)[i],
                        _ => (int)((uint*)indicesPtrTyped)[i]
                    };

                    byte* src = valuesPtr + i * elementSize;
                    byte* dst = dataPtr + currentIndex * elementSize;

                    for (int j = 0; j < elementSize; j++)
                        dst[j] = src[j];
                }
            }
        }

        return result;
    }

    private static MaterialData[] ReadMaterials(JsonElement materials, GLTFComponentsData components, RenderSettings settings)
    {
        MaterialData[] result = new MaterialData[materials.GetArrayLength()];
        int materialIndex = 0;

        foreach (var material in materials.EnumerateArray())
        {
            MaterialData currentMaterial = new();
            RenderSettings flags = RenderSettings.None;

            if (material.TryGetProperty("pbrMetallicRoughness", out JsonElement metallicRoughness))
            {
                if (metallicRoughness.TryGetProperty("baseColorFactor", out JsonElement baseColorFactor)
                    && (settings & RenderSettings.BaseColorFactor) != 0)
                {
                    float[] baseColor = baseColorFactor.Deserialize<float[]>()!;
                    currentMaterial.BaseColorFactor = new Color(baseColor[0], baseColor[1], baseColor[2], baseColor[3]);

                    flags |= RenderSettings.BaseColorFactor;
                }

                if (metallicRoughness.TryGetProperty("baseColorTexture", out JsonElement baseColorTex)
                    && (settings & RenderSettings.AlbedoMap) != 0)
                {
                    currentMaterial.AlbedoMap = ConfigureTexture(components.textures[baseColorTex.GetProperty("index").GetInt32()],
                                                                 TextureUnit.Texture0, PixelInternalFormat.Rgba8,
                                                                 baseColorTex.TryGetProperty("texCoord", out JsonElement texCoord)
                                                                 ? texCoord.GetInt32() : 0, RenderSettings.AlbedoMap);

                    flags |= RenderSettings.AlbedoMap;
                }

                if (metallicRoughness.TryGetProperty("metallicFactor", out JsonElement metallicFactor)
                    && (settings & RenderSettings.MetallicFactor) != 0)
                {
                    currentMaterial.MetallicFactor = metallicFactor.GetSingle();
                    flags |= RenderSettings.MetallicFactor;
                }

                if (metallicRoughness.TryGetProperty("roughnessFactor", out JsonElement roughnessFactor)
                    && (settings & RenderSettings.RoughnessFactor) != 0)
                {
                    currentMaterial.RoughnessFactor = roughnessFactor.GetSingle();
                    flags |= RenderSettings.RoughnessFactor;
                }

                if (metallicRoughness.TryGetProperty("metallicRoughnessTexture", out JsonElement metallicRoughnessTex)
                    && (settings & RenderSettings.MetallicRoughnessMap) != 0)
                {
                    currentMaterial.MetallicRoughnessMap =
                    ConfigureTexture(components.textures[metallicRoughnessTex.GetProperty("index").GetInt32()],
                                     TextureUnit.Texture4, PixelInternalFormat.Rg8,
                                     metallicRoughnessTex.TryGetProperty("texCoord", out JsonElement texCoord)
                                     ? texCoord.GetInt32() : 0, RenderSettings.MetallicRoughnessMap);

                    flags |= RenderSettings.MetallicRoughnessMap;
                }
            }

            if (material.TryGetProperty("normalTexture", out JsonElement normalTex)
                && (settings & RenderSettings.NormalMap) != 0)
            {
                float scale = normalTex.TryGetProperty("scale", out JsonElement scaleEL) ? scaleEL.GetSingle() : 1.0f;

                currentMaterial.NormalMap = (ConfigureTexture(components.textures[normalTex.GetProperty("index").GetInt32()],
                                                              TextureUnit.Texture2, PixelInternalFormat.Rgba8,
                                                              normalTex.TryGetProperty("texCoord", out JsonElement texCoord)
                                                              ? texCoord.GetInt32() : 0, RenderSettings.NormalMap), scale);

                flags |= RenderSettings.NormalMap;
            }

            if (material.TryGetProperty("occlusionTexture", out JsonElement occlusionTex)
                && (settings & RenderSettings.OcclusionMap) != 0)
            {
                float strength = occlusionTex.TryGetProperty("strength", out JsonElement strengthEl) ? strengthEl.GetSingle() : 1.0f;

                currentMaterial.OcclusionMap = (ConfigureTexture(components.textures[occlusionTex.GetProperty("index").GetInt32()],
                                                                 TextureUnit.Texture3, PixelInternalFormat.R8,
                                                                 occlusionTex.TryGetProperty("texCoord", out JsonElement texCoord)
                                                                 ? texCoord.GetInt32() : 0, RenderSettings.OcclusionMap), strength);

                flags |= RenderSettings.OcclusionMap;
            }

            if (material.TryGetProperty("emissiveTexture", out JsonElement emissiveTex)
                && (settings & RenderSettings.EmissiveMap) != 0)
            {
                currentMaterial.EmissiveMap = ConfigureTexture(components.textures[emissiveTex.GetProperty("index").GetInt32()],
                                                               TextureUnit.Texture1, PixelInternalFormat.Rgba8,
                                                               emissiveTex.TryGetProperty("texCoord", out JsonElement texCoord)
                                                               ? texCoord.GetInt32() : 0, RenderSettings.EmissiveMap);

                flags |= RenderSettings.EmissiveMap;
            }

            if (material.TryGetProperty("emissiveFactor", out JsonElement emissiveFactor)
                && (settings & RenderSettings.EmissiveFactor) != 0)
            {
                float[] emissive = emissiveFactor.Deserialize<float[]>()!;
                currentMaterial.EmissiveFactor = new Color(emissive[0], emissive[1], emissive[2]);

                flags |= RenderSettings.EmissiveFactor;
            }

            static TextureData ConfigureTexture(TextureData texture, TextureUnit unit, PixelInternalFormat pixelFormat, int texCoords, RenderSettings key)
            {
                Sampler sampler = texture.Options.Sampler;
                texture.TexCoords = texCoords;
                texture.Options = new()
                {
                    Sampler = sampler,
                    InternalFormat = pixelFormat,
                    Unit = unit,
                    Key = key
                };

                return texture;
            }

            currentMaterial.Flags = flags;
            result[materialIndex] = currentMaterial;

            materialIndex++;
        }

        return result;
    }

    private static TextureData[] ReadTextures(JsonElement textures, GLTFComponentsData components)
    {
        TextureData[] result = new TextureData[textures.GetArrayLength()];
        int textureIndex = 0;

        foreach (var texture in textures.EnumerateArray())
        {
            TextureOptions textureOptions = texture.TryGetProperty("sampler", out JsonElement samplerEl) ?
                new() { Sampler = components.samplers[samplerEl.GetInt32()] } : new() { Sampler = Sampler.DefaultSampler };

            result[textureIndex] = new()
            {
                Data = components.imagesData[texture.GetProperty("source").GetInt32()].ToArray(),
                Options = textureOptions
            };

            textureIndex++;
        }

        return result;
    }

    private static Sampler[] ReadSamplers(JsonElement samplers) 
    {
        Sampler[] result = new Sampler[samplers.GetArrayLength()];
        int samplerIndex = 0;

        foreach (var sampler in samplers.EnumerateArray())
        {
            result[samplerIndex] = new()
            {
                MagFilter = sampler.TryGetProperty("magFilter", out JsonElement magFilter) && magFilter.GetInt32() is
                9728 or 9729
                    ? magFilter.GetInt32()
                    : 9729,

                MinFilter = sampler.TryGetProperty("minFilter", out JsonElement minFilter) && minFilter.GetInt32() is
                9728 or 9729 or 9984 or 9985 or 9986 or 9987
                    ? minFilter.GetInt32()
                    : 9987,

                WrapS = sampler.TryGetProperty("wrapS", out JsonElement wrapS) && wrapS.GetInt32() is
                10497 or 33071 or 33648
                    ? wrapS.GetInt32()
                    : 10497,

                WrapT = sampler.TryGetProperty("wrapT", out JsonElement wrapT) && wrapT.GetInt32() is
                10497 or 33071 or 33648
                    ? wrapT.GetInt32()
                    : 10497
            };

            samplerIndex++;
        }

        return result;
    }

    private static Memory<byte>[] ReadImages(JsonElement images, BufferViewData[] bufferViews)
    {
        Memory<byte>[] result = new Memory<byte>[images.GetArrayLength()];
        int imageIndex = 0;

        foreach (var image in images.EnumerateArray())
        {
            if (image.TryGetProperty("bufferView", out JsonElement bufferView))
            {
                result[imageIndex] = bufferViews[bufferView.GetInt32()].data;
            }
            else
            {
                string uri = image.GetProperty("uri").GetString()!;
                int endURI = uri.IndexOf("base64,", StringComparison.Ordinal);

                if (endURI == -1)
                    throw new Exception();

                string base64 = uri[(endURI + "base64,".Length)..];
                result[imageIndex] = Convert.FromBase64String(base64);
            }

            imageIndex++;
        }

        return result;
    }

    private static MeshData[] ReadMeshes(JsonElement meshes, GLTFComponentsData components)
    {
        MeshData[] result = new MeshData[meshes.GetArrayLength()];
        int meshIndex = 0;

        foreach (var mesh in meshes.EnumerateArray())
        {
            JsonElement primitives = mesh.GetProperty("primitives");
            int primitiveIndex = 0;
            PrimitiveData[] meshPrimitives = new PrimitiveData[primitives.GetArrayLength()];

            foreach (var primitive in primitives.EnumerateArray())
            {
                meshPrimitives[primitiveIndex] = ReadPrimitive(primitive, components);
                primitiveIndex++;
            }

            result[meshIndex] = new(meshPrimitives);
            meshIndex++;
        }

        static unsafe PrimitiveData ReadPrimitive(JsonElement primitive, GLTFComponentsData components)
        {
            PrimitiveData result = new();
            Primitive.Options options = new();

            if (primitive.TryGetProperty("indices", out JsonElement indices))
            {
                AccessorData indicesAccessor = components.accessors[indices.GetInt32()];

                result.Indices = indicesAccessor.data;
                options.Count = indicesAccessor.count;
                options.DrawElementsType = (DrawElementsType)indicesAccessor.componentType;
                options.IsIndexedGeometry = true;
            }

            if (primitive.TryGetProperty("material", out JsonElement material))
                result.Material = components.materials[material.GetInt32()];

            options.Mode = primitive.TryGetProperty("mode", out JsonElement mode) ?
                (PrimitiveType)mode.GetInt32() : PrimitiveType.Triangles;

            JsonElement attributes = primitive.GetProperty("attributes");

            AccessorData verticesAccessor = components.accessors[attributes.GetProperty("POSITION").GetInt32()];
            result.Vertices = verticesAccessor.data;

            if (options.Count == 0)
                options.Count = verticesAccessor.count;

            if (attributes.TryGetProperty("NORMAL", out JsonElement normal))
                result.Normals = components.accessors[normal.GetInt32()].data;

            for (int i = 0; i < 5; i++)
            {
                if (attributes.TryGetProperty($"TEXCOORD_{i}", out JsonElement texCoord))
                    result.UVSets[i] = components.accessors[texCoord.GetInt32()].data;
                else
                    break;
            }

            result.Options = options;

            if (verticesAccessor.max is not null && verticesAccessor.min is not null)
            {
                result.Box = new((Vector3)verticesAccessor.min, (Vector3)verticesAccessor.max);
                return result;
            }

            Vector3 min = new(float.MaxValue);
            Vector3 max = new(float.MinValue);

            byte[] vertices = result.Vertices;

            fixed (void* verticesPtr = &vertices[0])
            {
                float* floatPtr = (float*)verticesPtr;
                int vertexCount = vertices.Length / (3 * sizeof(float));

                for (int i = 0; i < vertexCount; i++)
                {
                    Vector3 currentVertex = new(floatPtr[0], floatPtr[1], floatPtr[2]);
                    min = Vector3.ComponentMin(min, currentVertex);
                    max = Vector3.ComponentMax(max, currentVertex);

                    floatPtr += 3;
                }
            }

            result.Box = new(min, max);

            return result;
        }

        return result;
    }

    private static HashSet<int> ReadJoints(JsonElement skins)
    {
        HashSet<int> result = [];

        foreach (var skin in skins.EnumerateArray())
        {
            foreach (var joint in skin.GetProperty("joints").EnumerateArray())
            {
                result.Add(joint.GetInt32());
            }
        }

        return result;
    }
}
