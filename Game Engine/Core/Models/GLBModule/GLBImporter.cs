namespace Game_Engine.Core.Models.GLBModule;

internal class GLBImporter
{
    private readonly Dictionary<MetadataTypes, string> _metadata;
    private readonly GLBMultiScene _multiScene;
    private readonly Dictionary<string, object> _jsonChunk;
    private readonly BinaryReader _reader;

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
            var scenesElements = (Dictionary<string, object>[])_jsonChunk["scenes"];

            if (scenesElements.Length == 0)
                throw new Exception();

            return new(ReadScenes(scenesElements), indexOfDefaultScene);
        }
        catch (Exception)
        {
            throw new FileLoadException("Error. GLB-file is invalid");
        }
    }

    private static GLBScene[] ReadScenes(Dictionary<string, object>[] scenesElements)
    {
        GLBScene[] result;

        result = new GLBScene[scenesElements.Length];

        for (int i = 0; i < scenesElements.Length; i++)
        {
            var nodes = (int[])scenesElements[i]["nodes"];

            if (scenesElements[i].TryGetValue("name", out object? name))
                result[i] = ReadScene(nodes, (string)name);
            else
                result[i] = ReadScene(nodes, "None");
        }

        return result;
    }

    private static GLBScene ReadScene(int[] nodes, string name)
    {
        return null;
    }

    private Dictionary<string, object> ReadJsonChunk()
    {
        if (new string(_reader.ReadChars(4)) != "glTF")
            throw new FileLoadException("Error. GLB-file is invalid");

        var version = _reader.ReadUInt32();

        if (version != 2)
            throw new FileLoadException($"Error. Unsupported GLB-file version: ({version}). Only version 2.0 is currently supported");

        var fileSize = _reader.ReadUInt32();

        var chunkLength = _reader.ReadUInt32();
        Dictionary<string, object?> chunkData;

        if (new string(_reader.ReadChars(4)) == "JSON")
        {
            chunkData = JSONSerializer.JsonToObj(_reader.ReadBytes((int)chunkLength));
        }
        else
            throw new FileLoadException("Error. GLB-file is invalid");

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
