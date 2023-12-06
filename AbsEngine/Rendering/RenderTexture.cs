using AbsEngine.Rendering.OpenGL;
using Silk.NET.Maths;

namespace AbsEngine.Rendering;

internal interface IBackendRenderTexture : IDisposable
{
    void Bind();
    void UnBind();
    void BlitTo(IBackendRenderTexture rt);
}

public class RenderTexture : IDisposable
{
    static RenderTexture? _active;

    public Texture ColorTexture { get; private set; }
    public Texture DepthTexture { get; private set; }

    internal IBackendRenderTexture _backendRt = null!;

    public static RenderTexture? Active
    {
        get => _active;
        set => _active = value; 
    }

    public RenderTexture(int width, int height)
    {
        ColorTexture = new Texture(width, height);
        ColorTexture.SetMaxMips(1);
        ColorTexture.Update();

        DepthTexture = new Texture(width, height);
        DepthTexture.InternalFormat = Silk.NET.OpenGL.InternalFormat.DepthComponent32;
        DepthTexture.PixelFormat = Silk.NET.OpenGL.PixelFormat.DepthComponent;
        DepthTexture.SetMaxMips(1);
        DepthTexture.Update();

        switch (Game.Instance!.Graphics.GraphicsAPIs)
        {
            case GraphicsAPIs.OpenGL:
                _backendRt = new OpenGLRenderTexture(this);
                break;
            case GraphicsAPIs.D3D11:
                _backendRt = null!;
                throw new NotImplementedException();
        }
    }

    public RenderTexture(Vector2D<int> size) :
        this(size.X, size.Y) { }

    public void SetSize(int width, int height)
    {
        ColorTexture.SetSize(width, height);
        ColorTexture.Update();

        DepthTexture.SetSize(width, height);
        DepthTexture.Update();
    }

    public void SetSize(Vector2D<int> size)
        => SetSize(size.X, size.Y);

    public void Bind()
        => _backendRt?.Bind();

    public void UnBind()
        => _backendRt?.UnBind();

    public void BlitTo(RenderTexture rt)
        => _backendRt?.BlitTo(rt._backendRt);

    public void Dispose()
    {
        Game.Instance?.QueueDisposable(_backendRt);
        Game.Instance?.QueueDisposable(ColorTexture);
        Game.Instance?.QueueDisposable(DepthTexture);

        GC.SuppressFinalize(this);
    }

    ~RenderTexture() => Dispose();
}
