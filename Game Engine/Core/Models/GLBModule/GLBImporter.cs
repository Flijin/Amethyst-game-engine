namespace Game_Engine.Core.Models.GLBModule;

internal class GLBImporter
{
    private readonly Dictionary<MetadataTypes, string> _metadata;
    private readonly GLBMultiScene _multiScene;
    private readonly Dictionary<string, object> _jsonChunk;
    private readonly BinaryReader _reader;

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

    private GLBScene ReadScene(int[] sceneNodes, string? name = default)
    {
        GLBScene result;

        if (sceneNodes.Length == 1)
        {
            var nodes = ((object[])_jsonChunk["nodes"]).Cast<Dictionary<string, object>>().ToArray();
            var currentNode = nodes[sceneNodes[0]];
            var currentNodeInfo = new NodeInfo(currentNode);
            var currentNodeIndex = sceneNodes[0];

            while (currentNodeInfo.Name != "RootNode" && currentNodeInfo.Children?.Length == 1 && currentNodeInfo.Mesh is null)
            {
                currentNodeIndex = currentNodeInfo.Children[0];
                currentNode = nodes[currentNodeIndex];
                currentNodeInfo = new(currentNode);
            }

            if (currentNodeInfo.Children is not null && currentNodeInfo.Mesh is null)
            {
                var childrenCount = currentNodeInfo.Children.Length;
                var sceneModels = new GLBModel[childrenCount];

                for (int i = 0; i < childrenCount; i++)
                {
                    sceneModels[i] = ReadModel(currentNodeInfo.Children[i]);
                }

                result = new GLBScene(sceneModels);
            }
            else if (currentNodeInfo.Mesh is not null && currentNodeInfo.Children is null)
            {
                result = new GLBScene([ReadModel(currentNodeIndex)]);
            }
            else
            {
                throw new Exception();
            }
        }
        else if (sceneNodes.Length > 1)
        {
            var sceneModels = new GLBModel[sceneNodes.Length];

            for (int i = 0; i < sceneNodes.Length; i++)
            {
                sceneModels[i] = ReadModel(sceneNodes[i]);
            }

            result = new GLBScene(sceneModels);
        }
        else
        {
            result = new GLBScene([]);
        }

        if (name is not null)
            result.Name = name;

        return result;
    }

    private static GLBModel ReadModel(int node)
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

        FileSize = _reader.ReadUInt32();

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
