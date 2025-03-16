namespace Amethyst_game_engine.Models.GLBModule;

public readonly struct GLBModel : IModel
{
	private readonly NodeInfo[] _nodes;
	private readonly Mesh[] meshes;

    public string Name { readonly get; init; } = "None";

    internal GLBModel(NodeInfo[] nodes, Mesh[] meshes)
	{
		_nodes = nodes;
		this.meshes = meshes;
	}

    Mesh[] IModel.GetMeshes() => meshes;

    bool IModel.UseMeshMatrix() => true;

    void IModel.RebuildShaders(uint renderKeys)
    {
        foreach (var mesh in meshes)
        {
            mesh.RebuildShaders(renderKeys, 1 << 24);
        }
    }

    public void Dispose()
    {
        foreach (var mesh in meshes)
        {
            mesh.Dispose();
        }

        foreach (var node in _nodes)
        {
            node.Dispose();
        }
    }
}
