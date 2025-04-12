using System.Runtime.CompilerServices;

namespace Amethyst_game_engine.Models;

internal interface IModel : IDisposable
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Mesh[] GetMeshes();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool UseMeshMatrix();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void RebuildShaders(uint renderKeys);
}
