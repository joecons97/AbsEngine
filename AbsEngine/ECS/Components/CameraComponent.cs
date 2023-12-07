using AbsEngine.Exceptions;
using AbsEngine.Maths;
using AbsEngine.Physics;
using AbsEngine.Rendering;
using Silk.NET.Maths;

namespace AbsEngine.ECS.Components;

public class CameraComponent : Component
{
    public bool IsMainCamera { get; internal set; } = true;

    public float FieldOfView { get; set; } = 65;

    public float NearClipPlane = 0.01f;
    public float FarClipPlane = 1000f;

    public Matrix4X4<float> GetViewProjectMatrix()
    {
        if(Game.Instance == null)
            throw new GameInstanceException();

        Matrix4X4.Invert(Entity.Transform.WorldMatrix, out var viewMat);
        var frameSize = RenderTexture.Active?.ColorTexture.Size ?? Game.Instance.Window.FramebufferSize;

        var projMat = Matrix4X4.CreatePerspectiveFieldOfView(FieldOfView * AbsMaths.DEG_2_RAD,
        (float)frameSize.X / (float)frameSize.Y, NearClipPlane, FarClipPlane);

        return viewMat * projMat;
    }

    public Frustum GetFrustum()
    {
        return new Frustum(GetViewProjectMatrix());
    }
}

public class SceneCameraComponent : CameraComponent
{
    public static bool IsInSceneView = false;
}