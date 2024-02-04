using AbsEngine.ECS;
using AbsGameProject.Components.Terrain;
using AbsGameProject.Jobs;

namespace AbsGameProject.Systems.Terrain
{
    public class TerrainMeshConstructorSystem : ComponentSystem<TerrainChunkComponent>
    {
        //protected override int MaxIterationsPerFrame => 50;

        public override bool UseJobSystem => false;

        public TerrainMeshConstructorSystem(Scene scene) : base(scene)
        {
        }

        public override void OnTick(TerrainChunkComponent component, float deltaTime)
        {
            if (component.IsReadyForMeshGeneration == false
                && ((component.State == TerrainChunkComponent.TerrainState.Done && component.IsAwaitingRebuild) == false))
                return;

            component.IsAwaitingRebuild = false;

            if(component.IsFull)
                Scene.Game.Scheduler.Schedule(new ChunkMeshBuildJob(component));
            else
                Scene.Game.Scheduler.Schedule(new ChunkMeshBuildLodJob(component));

            Scene.Game.Scheduler.Flush();
        }
    }
}
