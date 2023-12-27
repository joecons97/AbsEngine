using AbsEngine.ECS.Components;

namespace AbsEngine.Rendering.RenderCommand.Internal.MultiDrawRenderCommand;

internal interface IMultiDrawRenderCommand<T> :IDisposable where T : unmanaged
{
    void SetMaterial(Material mat);
    void SetMaterialBufferObjects(T[]? bufferData);
    void SetDrawBuffer(DrawBuffer buffer);
    void SetDrawCommands(DrawArraysIndirectCommand[] commands);
    void Render(CameraComponent camera, RenderTexture target);
}
