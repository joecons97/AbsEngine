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

public class TerrainLodChunkQueuerSystem : ComponentSystem<TerrainLodChunkComponent>
{
    protected override int MaxIterationsPerFrame => 50;

    CameraComponent _cam;

    public TerrainLodChunkQueuerSystem(Scene scene) : base(scene)
    {
        _cam = Scene.EntityManager.GetFirstOrDefault<CameraComponent>(x => x.IsMainCamera) ?? throw new Exception();
    }

    public override void OnTick(TerrainLodChunkComponent component, float deltaTime)
    {
        var chunk = component.Data;
        if (chunk != null && chunk.State == TerrainChunkComponent.TerrainState.MeshConstructed)
        {
            if (chunk.CanSee(_cam.GetFrustum()))
                TerrainChunkBatcherRenderer.QueueChunkForBatching(chunk);
            else
                chunk.IsWaitingForLookAt = true;
        }
    }
}