using System.Diagnostics.CodeAnalysis;

namespace Amethyst_game_engine.Models.New_classes;

internal sealed class PrimitiveData
{
    public required byte[] Vertices { get; set; }
    public byte[]? Normals { get; set; }
    public byte[]? Colors { get; set; }
    public MaterialData? Material { get; set; }
}
