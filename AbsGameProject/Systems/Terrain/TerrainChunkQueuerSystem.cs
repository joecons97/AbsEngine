using AbsEngine.ECS;
using AbsGameProject.Components.Terrain;

namespace AbsGameProject.Systems.Terrain;

public class TerrainChunkQueuerSystem : ComponentSystem<TerrainChunkComponent>
{
    protected override int MaxIterationsPerFrame => 50;

    public TerrainChunkQueuerSystem(Scene scene) : base(scene)
    {
    }

    public override void OnTick(TerrainChunkComponent component, float deltaTime)
    {
        if(component.State == TerrainChunkComponent.TerrainState.MeshConstructed)
        {
            TerrainChunkBatcherRenderer.QueueChunkForBatching(component);
        }
    }
}
