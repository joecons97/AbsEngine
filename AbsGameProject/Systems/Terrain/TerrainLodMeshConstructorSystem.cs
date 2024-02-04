using AbsEngine.ECS;
using AbsGameProject.Components.Terrain;
using AbsGameProject.Jobs;

namespace AbsGameProject.Systems.Terrain
{
    public class TerrainLodMeshConstructorSystem : ComponentSystem<TerrainLodChunkComponent>
    {
        //protected override int MaxIterationsPerFrame => 50;

        public override bool UseJobSystem => false;

        public TerrainLodMeshConstructorSystem(Scene scene) : base(scene)
        {
        }

        public override void OnTick(TerrainLodChunkComponent component, float deltaTime)
        {
            var chunk = component.Data;
            if (chunk == null || chunk.IsReadyForMeshGeneration == false
                && ((chunk.State == TerrainChunkComponent.TerrainState.Done && chunk.IsAwaitingRebuild) == false))
                return;

            chunk.IsAwaitingRebuild = false;

            Scene.Game.Scheduler.Schedule(new ChunkMeshBuildLodJob(chunk));

            Scene.Game.Scheduler.Flush();
        }
    }
}
