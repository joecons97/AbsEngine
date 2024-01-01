using AbsEngine.ECS;
using AbsGameProject.Components.Terrain;

namespace AbsGameProject.Systems.Terrain;

internal class TerrainChunkQueuerSystem : ComponentSystem<TerrainChunkComponent>
{
    protected override Func<TerrainChunkComponent, bool>? Predicate => x => 
        x.State == TerrainChunkComponent.TerrainState.MeshConstructed;

    protected override int MaxIterationsPerFrame => 1;

    public TerrainChunkQueuerSystem(Scene scene) : base(scene)
    {
    }

    public override void OnTick(TerrainChunkComponent component, float deltaTime)
    {
        TerrainChunkBatcherRenderer.QueueChunkForBatching(component);
    }
}
