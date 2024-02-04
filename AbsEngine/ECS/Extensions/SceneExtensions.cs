using AbsEngine.ECS.Components;
using AbsEngine.ECS.Systems;

namespace AbsEngine.ECS.Extensions;

public static class SceneExtensions
{
    public static void RegisterMeshRenderer(this Scene scene)
    {
        scene.RegisterSystem<MeshRendererSystem>();
    }

    public static void RegisterSceneCamera(this Scene scene, float cameraSpeed = 15)
    {
        var sceneCam = scene.EntityManager.CreateEntity("Scene Camera");
        var cam = sceneCam.AddComponent<SceneCameraComponent>();
        cam.MoveSpeed = cameraSpeed;

        scene.RegisterSystem<SceneCameraSystem>();
    }
}
