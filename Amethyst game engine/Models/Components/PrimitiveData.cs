using System.Diagnostics.CodeAnalysis;
using Amethyst_game_engine.Core.GameObjects.Components;

namespace Amethyst_game_engine.Models.Components;

internal sealed class PrimitiveData
{
    [AllowNull]
    public byte[] Vertices { get; set; }
    public byte[]? Normals { get; set; }
    public byte[]? Colors { get; set; }
    public MaterialData? Material { get; set; }
    public Primitive.Options Options { get; set; }
}
