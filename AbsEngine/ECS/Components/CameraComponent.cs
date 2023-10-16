namespace AbsEngine.ECS.Components;

public class CameraComponent : Component
{
    public bool IsMainCamera { get; internal set; } = true;

    public float FieldOfView { get; set; } = 65;

    public float NearClipPlane = 0.01f;
    public float FarClipPlane = 1000f;
}
