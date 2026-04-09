using System.Text;
using System.Text.Json;
using Amethyst_game_engine.Core.GameObjects.Components;
using Amethyst_game_engine.Core.Render.Settings;
using Amethyst_game_engine.Core.Utilities;
using Amethyst_game_engine.Models.Components;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Amethyst_game_engine.Models.GLBModule;

public static class GLBImporter
{
    private struct GLTFComponentsData
    {
        public BufferViewData[] bufferViews;
        public AccessorData[] accessors;
    }

    private struct AccessorData
    {
        public byte[] data;
        public int componentType;
        public bool normalized;
    }

    private struct BufferViewData
    {
        public Memory<byte> data;
        public int stride;
    }

    private struct TextureData
    {
        public TextureOptions sampler;
        public Memory<byte> data;
    }

    public static GLBModelData? ReadModel(string path, RenderSettings settings = RenderSettings.All)
    {
        if (File.Exists(path) == false)
        {
            System.PrintMessage($"Error. GLB-file {path} does not exists", MessageTypes.ErrorMessage);
            return null;
        }

        using FileStream stream = File.OpenRead(path);
        using BinaryReader reader = new(stream);

        if (stream.Length > 1024L * 1024L * 1024L)
        {
            System.PrintMessage($"Error. GLB-file {path} is too big. Max supported size: 1 GB", MessageTypes.ErrorMessage);
            return null;
        }

        //try
        //{
            return ReadFile(reader, path);
        //}
        //catch (Exception)
        //{
        //    System.PrintMessage($"Error. GLB-file {path} is invalid", MessageTypes.ErrorMessage);
        //    return null;
        //}
    }

    private static GLBModelData? ReadFile(BinaryReader reader, string path)
    {
        var magic = reader.ReadUInt32();

        if (magic != 0x46546C67)
        {
            System.PrintMessage($"Error. GLB-file {path} is not valid", MessageTypes.ErrorMessage);
            return null;
        }

        void PrintUnsupportedVersion(object version)
        {
            System.PrintMessage($"Error. Unsupported GLB-file {path} version: ({version}). Only version 2.x is currently supported", MessageTypes.ErrorMessage);
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
            System.PrintMessage($"Error. GLB-file {path} is not valid", MessageTypes.ErrorMessage);
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

        var result = ReadModelData(jsonChunk, binChunk);

        if (result is null)
            return null;

        if (asset.TryGetProperty("extras", out JsonElement extras))
            ReadExtras(extras, result);

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
        {
            return reader.ReadBytes((int)chunkLength);
        }
        else
        {
            return null;
        }
    }

    private static GLBModelData? ReadModelData(JsonElement jsonChunk, byte[] binChunk)
    {
        GLBScene[] scenes;

        byte[][]? buffers = ReadBuffers(jsonChunk.GetProperty("buffers"), binChunk);

        if (buffers is null)
            return null;

        GLTFComponentsData componentsData = new()
        {
            bufferViews = ReadBufferViews(jsonChunk.GetProperty("bufferViews"), buffers),
        };

        componentsData.accessors = ReadAccessors(jsonChunk.GetProperty("accessors"), componentsData.bufferViews);

        JsonElement scenesJson = jsonChunk.GetProperty("scenes");
        scenes = new GLBScene[scenesJson.GetArrayLength()];

        int sceneIndex = 0;

        foreach (var scene in scenesJson.EnumerateArray())
        {
            string? sceneName;

            if (scene.TryGetProperty("name", out JsonElement sceneNameEl))
                sceneName = sceneNameEl.GetString();
            else
                sceneName = null;

            scenes[sceneIndex] = ReadScene(jsonChunk, sceneName, scene.GetProperty("nodes").EnumerateArray());
        }

        return new GLBModelData(scenes);
    }

    private static GLBScene ReadScene(JsonElement chunk, string? sceneName, JsonElement.ArrayEnumerator sceneNodes)
    {
        return null;
    }

    private static void ReadExtras(JsonElement extras, GLBModelData model)
    {
        if (extras.TryGetProperty("author", out JsonElement author))
            model.Author = author.GetString();

        if (extras.TryGetProperty("license", out JsonElement license))
            model.License = license.GetString();

        if (extras.TryGetProperty("source", out JsonElement source))
            model.Sourse = source.GetString();

        if (extras.TryGetProperty("title", out JsonElement title))
            model.Title = title.GetString();
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
                var indexOfEndURI = uriString.IndexOf("base64,", minMediatypeLength, StringComparison.Ordinal) + "base64,".Length;

                if (indexOfEndURI == -1)
                    return null;

                var base64 = uriString[indexOfEndURI..];
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
        int indexOfBufferView = 0;

        foreach (var bufferView in bufferViews.EnumerateArray())
        {
            int bufferIndex = bufferView.GetProperty("buffer").GetInt32();
            int byteLength = bufferView.GetProperty("byteLength").GetInt32();
            int byteOffset = bufferView.TryGetProperty("byteOffset", out JsonElement byteOffsetEl) ? byteOffsetEl.GetInt32() : 0;
            int byteStride = bufferView.TryGetProperty("byteStride", out JsonElement byteStrideEl) ? byteStrideEl.GetInt32() : 0;

            result[indexOfBufferView] = new()
            {
                data = new Memory<byte>(buffers[bufferIndex], byteOffset, byteLength),
                stride = byteStride
            };

            indexOfBufferView++;
        }

        return result;
    }

    private static AccessorData[] ReadAccessors(JsonElement accessors, BufferViewData[] bufferViews)
    {
        var result = new AccessorData[accessors.GetArrayLength()];
        int indexOfAccessor = 0;

        foreach (var accessor in accessors.EnumerateArray())
        {
            int componentType = accessor.GetProperty("componentType").GetInt32();
            int count = accessor.GetProperty("count").GetInt32();
            string type = accessor.GetProperty("type").GetString()!;

            int elementSize = Helpers.GetNumberOfComponents(type) * Helpers.SizeOfComponent(componentType);

            int byteOffset = accessor.TryGetProperty("byteOffset", out JsonElement byteOffsetEl) ? byteOffsetEl.GetInt32() : 0;
            bool normalized = accessor.TryGetProperty("normalized", out JsonElement normalizedEl) && normalizedEl.GetBoolean();
            int bufferView = accessor.TryGetProperty("bufferView", out JsonElement bufferViewEl) ? bufferViewEl.GetInt32() : -1;

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

            result[indexOfAccessor] = new()
            {
                data = data,
                componentType = componentType,
                normalized = normalized,
            };

            indexOfAccessor++;
        }

        static byte[] CopyData(BufferViewData bufferView, int offset, int elementSize, int count)
        {
            if (bufferView.stride == 0)
                return bufferView.data.Slice(offset, elementSize * count).ToArray();

            byte[] result = new byte[elementSize * count];

            int srsOffset = offset;
            int dstOffset = 0;

            for (int i = 0; i < count; i++)
            {
                bufferView.data.Slice(srsOffset, elementSize).CopyTo(result.AsMemory(dstOffset));
                dstOffset += elementSize;
                srsOffset += elementSize + bufferView.stride;
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

    private static MaterialData[] ReadMaterials(JsonElement materials, AccessorData[] accessors)
    {
        return [];
    }

    private static TextureData[] ReadTextures(
        JsonElement textures,
        int[] imagesBuffViews,
        BufferViewData[] bufferViews,
        TextureOptions[] samplers)
    {
        TextureData[] result = new TextureData[textures.GetArrayLength()];
        int indexOfTexture = 0;

        foreach (var texture in textures.EnumerateArray())
        {
            BufferViewData bufferView = bufferViews[imagesBuffViews[texture.GetProperty("source").GetInt32()]];
            TextureOptions sampler = texture.TryGetProperty("sampler", out JsonElement samplerEl) ?
                samplers[samplerEl.GetInt32()] : new() { Sampler = Sampler.DefaultSampler };

            result[indexOfTexture] = new()
            {
                data = bufferView.data.ToArray(),
                sampler = sampler
            };

            indexOfTexture++;
        }

        return result;
    }

    private static Sampler[] ReadSamplers(JsonElement samplers) 
    {
        Sampler[] result = new Sampler[samplers.GetArrayLength()];
        int indexOfSampler = 0;

        foreach (var sampler in samplers.EnumerateArray())
        {
            result[indexOfSampler] = new()
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

            indexOfSampler++;
        }

        return result;
    }
}
