using Amethyst_game_engine.Core;
using Amethyst_game_engine.Render;
using OpenTK.Graphics.OpenGL;
using StbImageSharp;
using System.Runtime.CompilerServices;

namespace Amethyst_game_engine.Models.GLBModule;

public class GLBImporter
{
    private const float MAX_SUPPORTED_VERSION = 2.0f;
    private const uint USE_MESH_MATRIX = 1 << 24;

    private readonly Dictionary<string, object> _jsonChunk;
    private byte[] _binChunk = [];

    private readonly Dictionary<MetadataTypes, string>? _metadata;
    private readonly GLBMultiScene _multiScene;

    private string[] _extensions = [];
    private string[] _extensionsRequired = [];

    private int[] _glTextures = [];

    private readonly uint _renderSettings;

    private static readonly Dictionary<string, bool> _supportedExtensions = new()
    {
        { "KHR_materials_specular", false },
        { "KHR_materials_emissive_strength", false },
        { "KHR_materials_unlit", false }
    };

    public GLBImporter(string path) : this(path, RenderSettings.All) { }

    public GLBImporter(string path, RenderSettings settings)
    {
        _renderSettings = (uint)settings & (uint)Window.RenderKeys;

        BinaryReader reader = new(new FileStream(path, FileMode.Open));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GLBMultiScene GetMultiScene() => _multiScene;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        var asset = (Dictionary<string, object>)_jsonChunk["asset"];

        if (((string)asset["version"])[0] != '2')
            SystemSettings.PrintMessage($"The file glTF is { asset["version"] } version. Max 2.0 is supported.", MessageTypes.WarningMessage);

        if (asset.TryGetValue("minVersion", out object? minVersion) &&
            float.Parse((string)minVersion) > MAX_SUPPORTED_VERSION)
        {
            SystemSettings.PrintMessage($"Error. The file requires glTF {minVersion} support. Max 2.0 is supported.", MessageTypes.ErrorMessage);
        }

        if (asset.TryGetValue("generator", out object? generagor))
            result.Add(MetadataTypes.Generator, (string)generagor);

        if (asset.TryGetValue("copyright", out object? copyright))
            result.Add(MetadataTypes.Copyright, (string)copyright);

        return result;
    }

    private static void ReadFile(BinaryReader reader)
    {
        if (reader.ReadUInt32() != 0x46546C67)
            SystemSettings.PrintMessage($"Error. GLB-file is invalid", MessageTypes.ErrorMessage);

        var version = reader.ReadUInt32();

        if (version != 2)
            SystemSettings.PrintMessage($"Error. Unsupported GLB-file version: ({version}). Only version 2.x is currently supported", MessageTypes.ErrorMessage);

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
