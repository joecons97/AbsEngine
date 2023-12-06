using AbsEngine.Exceptions;
using Silk.NET.OpenGL;

namespace AbsEngine.Rendering.OpenGL;

internal class OpenGLRenderTexture : IBackendRenderTexture
{
    private GL _gl;
    private RenderTexture _rt;
    private uint _handle;

    public OpenGLRenderTexture(RenderTexture rt)
    {
        _rt = rt;

        _gl = ((OpenGLGraphics)Game.Instance!.Graphics).Gl;
        _handle = _gl.GenFramebuffer();

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _handle);
        _gl.DrawBuffer(DrawBufferMode.ColorAttachment0);

        var openGlCol = (OpenGLTexture)_rt.ColorTexture._backendTexture;
        var openGlDepth = (OpenGLTexture)_rt.DepthTexture._backendTexture;

        _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, GLEnum.Texture2D, openGlCol.Handle, 0);
        _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, GLEnum.Texture2D, openGlDepth.Handle, 0);

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void Bind()
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _handle);
        _gl.Viewport(_rt.ColorTexture.Size);
    }

    public void UnBind()
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        if (Game.Instance == null)
            throw new GameInstanceException();

        _gl.Viewport(Game.Instance.Window.FramebufferSize);
    }

    public void Dispose()
    {
        UnBind();
        _gl.DeleteFramebuffer(_handle);
    }

    public void BlitTo(IBackendRenderTexture rt)
    {
        var oglRt = (OpenGLRenderTexture)rt;

        _gl.BlitNamedFramebuffer(_handle, oglRt._handle,
            0, 0, _rt.ColorTexture.Size.X, _rt.ColorTexture.Size.Y,
            0, 0, oglRt._rt.ColorTexture.Size.X, oglRt._rt.ColorTexture.Size.Y,
            ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
    }
}
