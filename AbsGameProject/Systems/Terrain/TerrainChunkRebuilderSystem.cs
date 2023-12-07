using AbsEngine.ECS;
using AbsGameProject.Components.Terrain;

namespace AbsGameProject.Systems.Terrain;

internal class TerrainChunkRebuilderSystem : ComponentSystem<TerrainChunkComponent>
{
    protected override Func<TerrainChunkComponent, bool>? Predicate => x => x.State == TerrainChunkComponent.TerrainState.Done && x.IsAwaitingRebuild;

    public TerrainChunkRebuilderSystem(Scene scene) : base(scene)
    {
    }

    public override void OnTick(TerrainChunkComponent component, float deltaTime)
    {
        component.IsAwaitingRebuild = false;
        component.State = TerrainChunkComponent.TerrainState.Decorated;
    }
}
