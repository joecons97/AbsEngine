using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsGameProject.Components.Terrain;

namespace AbsGameProject.Systems.Terrain;

public class TerrainChunkQueuerSystem : ComponentSystem<TerrainChunkComponent>
{
    protected override int MaxIterationsPerFrame => 50;

    CameraComponent _cam;

    public TerrainChunkQueuerSystem(Scene scene) : base(scene)
    {
        _cam = Scene.EntityManager.GetFirstOrDefault<CameraComponent>(x => x.IsMainCamera) ?? throw new Exception();
    }

    public override void OnTick(TerrainChunkComponent component, float deltaTime)
    {
        if(component.State == TerrainChunkComponent.TerrainState.MeshConstructed)
        {
            if (component.CanSee(_cam.GetFrustum()))
                TerrainChunkBatcherRenderer.QueueChunkForBatching(component);
            else
                component.IsWaitingForLookAt = true;
        }
    }
}