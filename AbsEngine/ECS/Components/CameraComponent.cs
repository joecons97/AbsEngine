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

    public Matrix4X4<float> GetViewMatrix()
    {
        if(Game.Instance == null)
            throw new GameInstanceException();

        Matrix4X4.Invert(Entity.Transform.WorldMatrix, out var viewMat);

        return viewMat;
    }

    public Matrix4X4<float> GetProjectionMatrix()
    {
        if (Game.Instance == null)
            throw new GameInstanceException();

        var frameSize = RenderTexture.Active?.ColorTexture.Size ?? Game.Instance.Window.FramebufferSize;

        var projMat = Matrix4X4.CreatePerspectiveFieldOfView(FieldOfView * AbsMaths.DEG_2_RAD,
        (float)frameSize.X / (float)frameSize.Y, NearClipPlane, FarClipPlane);

        return projMat;
    }

    public Matrix4X4<float> GetViewProjectionMatrix()
        => GetViewMatrix() * GetProjectionMatrix();

    public Frustum GetFrustum()
    {
        return new Frustum(GetViewMatrix() * GetProjectionMatrix());
    }
}

public class SceneCameraComponent : CameraComponent
{
    public static bool IsInSceneView = false;
}