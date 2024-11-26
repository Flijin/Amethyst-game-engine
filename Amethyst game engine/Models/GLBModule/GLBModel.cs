namespace Amethyst_game_engine.Models.GLBModule;

public readonly struct GLBModel
{
	private readonly NodeInfo?[] _nodes;
	internal readonly Mesh[] meshes;
	internal readonly int _renderProfile = 0b_0010;

    public string Name { readonly get; init; } = "None";

    internal GLBModel(NodeInfo?[] nodes, Mesh[] meshes)
	{
		_nodes = nodes;
		this.meshes = meshes;
	}
}
