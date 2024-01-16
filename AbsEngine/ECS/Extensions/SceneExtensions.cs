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
        sceneCam.Transform.Position = new Silk.NET.Maths.Vector3D<float>(0, 50, 0);
        var cam = sceneCam.AddComponent<SceneCameraComponent>();
        cam.FarClipPlane = 5000;

        scene.RegisterSystem<SceneCameraSystem>();
    }
}
