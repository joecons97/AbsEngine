using AbsEngine.ECS.Components;
using AbsEngine.Physics;
using AbsEngine.Rendering;

internal interface IRenderCommand
{
    public int RenderQueuePosition { get; }

    void Render(IGraphics graphics, CameraComponent camera, RenderTexture target);

    public bool ShouldCull(Frustum frustum);
}