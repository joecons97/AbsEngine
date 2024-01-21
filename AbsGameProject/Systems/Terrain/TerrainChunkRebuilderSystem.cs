using AbsEngine.ECS;
using AbsGameProject.Components.Terrain;

namespace AbsGameProject.Systems.Terrain;

internal class TerrainChunkRebuilderSystem : ComponentSystem<TerrainChunkComponent>
{
    protected override int MaxIterationsPerFrame => 1;

    protected override bool UseParallel => true;

    public TerrainChunkRebuilderSystem(Scene scene) : base(scene)
    {
    }

    public override void OnTick(TerrainChunkComponent component, float deltaTime)
    {
        if ((component.State == TerrainChunkComponent.TerrainState.Done && component.IsAwaitingRebuild) == false)
            return;

        component.IsAwaitingRebuild = false;
        component.State = TerrainChunkComponent.TerrainState.Decorated;
    }
}
