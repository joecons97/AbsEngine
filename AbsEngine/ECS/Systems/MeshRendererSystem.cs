using AbsEngine.ECS.Components;
using AbsEngine.Maths;
using Silk.NET.Maths;

namespace AbsEngine.ECS.Systems;

public class MeshRendererSystem : ComponentSystem<MeshRendererComponent>
{
    CameraComponent? mainCamera;
    Matrix4X4<float> viewMat;
    Matrix4X4<float> projMat;
    Matrix4X4<float> vpMat;

    public MeshRendererSystem(Scene scene) : base(scene)
    {
    }

    public override void OnInitialiseTick(float deltaTime)
    {
        mainCamera = Scene.EntityManager.GetComponents<CameraComponent>(x => x.IsMainCamera).FirstOrDefault();
        if (mainCamera == null) return;

        var trans = mainCamera.Entity.GetComponent<TransformComponent>();
        if (trans == null) return;

        viewMat = Matrix4X4.CreateLookAt(trans.LocalPosition, trans.LocalPosition + trans.Forward, trans.Up);

        var winX = (float)Game.Instance!.Window.FramebufferSize.X;
        var winY = (float)Game.Instance!.Window.FramebufferSize.Y;

        projMat = Matrix4X4.CreatePerspectiveFieldOfView(mainCamera.FieldOfView * AbsMaths.DEG_2_RAD,
                    winX / winY, mainCamera.NearClipPlane, mainCamera.FarClipPlane);

        vpMat = viewMat * projMat;
    }

    public override void OnTick(MeshRendererComponent component, float deltaTime)
    {
        var trans = component.Entity.Transform;

        component.Shader?.Bind();
        component.Mesh?.Bind();

        var world = trans.WorldMatrix;

        component.Shader?.SetMatrix("uWorldMatrix", world);
        component.Shader?.SetMatrix("uMvp", world * vpMat);

        Game.Instance!.Graphics.DrawElements((uint)component.Mesh?.Triangles.Length!);
    }
}
