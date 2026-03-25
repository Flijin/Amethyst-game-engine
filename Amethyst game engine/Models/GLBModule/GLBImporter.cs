using System.Text.Json;
using Amethyst_game_engine.Core.Render.Settings;
using Amethyst_game_engine.Core.Utilities;

namespace Amethyst_game_engine.Models.GLBModule;

public static class GLBImporter
{
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

        try
        {
            return ReadFile(reader, path);
        }
        catch (Exception)
        {
            System.PrintMessage($"Error. GLB-file {path} is invalid", MessageTypes.ErrorMessage);
            return null;
        }
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

        var buffers = ReadBuffers(jsonChunk.GetProperty("buffers"), binChunk);

        var result = ReadModelData(jsonChunk, buffers);

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

    private static byte[][] ReadBuffers(JsonElement buffers, byte[] binChunk)
    {
        var result = new byte[buffers.GetArrayLength()][];
        int bufferIndex = 0;

        foreach (var buffer in buffers.EnumerateArray())
        {
            if (buffer.TryGetProperty("uri", out JsonElement uri))
            {
                var base64 = uri.GetString()!.Substring("data:application/octet-stream;base64,".Length);
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

    private static GLBModelData ReadModelData(JsonElement chunk, byte[][] buffers)
    {
        GLBScene[] scenes;

        var scenesJson = chunk.GetProperty("scenes");
        scenes = new GLBScene[scenesJson.GetArrayLength()];

        int sceneIndex = 0;

        foreach (var scene in scenesJson.EnumerateArray())
        {
            string? sceneName;

            if (scene.TryGetProperty("name", out JsonElement sceneNameEl))
                sceneName = sceneNameEl.GetString();
            else
                sceneName = null;

            scenes[sceneIndex] = ReadScene(chunk, sceneName, scene.GetProperty("nodes").EnumerateArray());
        }

        return new GLBModelData(scenes);
    }

    private static GLBScene ReadScene(JsonElement chunk, string? sceneName, JsonElement.ArrayEnumerator sceneNodes)
    {
        return null;
    }
}
