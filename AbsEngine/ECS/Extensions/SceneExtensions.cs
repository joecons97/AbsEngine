using AbsEngine.ECS.Components;
using AbsEngine.ECS.Systems;

namespace AbsEngine.ECS.Extensions;

public static class SceneExtensions
{
    public static void RegisterMeshRenderer(this Scene scene)
    {
        scene.RegisterSystem<MeshRendererSystem>();
    }

    public static void RegisterSceneCamera(this Scene scene)
    {
        var sceneCam = scene.EntityManager.CreateEntity("Scene Camera");
        sceneCam.AddComponent<SceneCameraComponent>();

        scene.RegisterSystem<SceneCameraSystem>();
    }
}
