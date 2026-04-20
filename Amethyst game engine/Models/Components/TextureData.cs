using Amethyst_game_engine.Core.GameObjects.Components;

namespace Amethyst_game_engine.Models.Components;

internal struct TextureData
{
    public TextureOptions Options { get; set; }
    public byte[] Data { get; set; }
    public int TexCoords { get; set; }
}
