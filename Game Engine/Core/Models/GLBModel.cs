using System.Text;

namespace Game_Engine.Core.Models;

internal class GLBModel
{
    private uint _fileSize;

    public GLBModel(string path)
    {
        using BinaryReader reader = new(File.OpenRead(path));
        ReadModel(reader);
    }

    private void ReadModel(BinaryReader reader)
    {
        if (new string(reader.ReadChars(4)) != "glTF")
            throw new FileLoadException("Error. GLB-file is not valid");

        var version = reader.ReadUInt32();

        if (version != 2)
            throw new FileLoadException($"Error. unsupported GLB-file version: ({version}). Only version 2 is currently supported");

        _fileSize = reader.ReadUInt32();

        var chunkLength = reader.ReadUInt32();
        Dictionary<string, object?> chunkData;

        if (new string(reader.ReadChars(4)) == "JSON")
        {
            chunkData = JSONSerializer.JsonToObj(Encoding.UTF8.GetString(reader.ReadBytes((int)chunkLength)));
        }
        else throw new FileLoadException("Error. GLB-file is corrupted");
    }
}
