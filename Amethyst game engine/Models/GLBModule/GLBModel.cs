namespace Amethyst_game_engine.Models.GLBModule;

public readonly struct GLBModel
{
	private readonly NodeInfo?[] _nodes;
	internal readonly Mesh[] _meshes;

	public string Name { readonly get; init; } = "None";

    internal GLBModel(NodeInfo?[] nodes, Mesh[] meshes)
	{
		_nodes = nodes;
		_meshes = meshes;
	}
}
