using AbsEngine.Rendering.OpenGL;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace AbsEngine.Rendering;

public enum TextureWrapMode
{
    LinearMipmapLinear,
    Repeat,
    ClampToBorder,
    ClampToBorderArb,
    ClampToBorderNV,
    ClampToBorderSgis,
    ClampToEdge,
    ClampToEdgeSgis,
    MirroredRepeat
}

public enum TextureMinFilter
{
    Nearest,
    Linear,
    NearestMipmapNearest,
    LinearMipmapNearest,
    NearestMipmapLinear,
    LinearMipmapLinear,
    Filter4Sgis,
    LinearClipmapLinearSgix,
    PixelTexGenQCeilingSgix,
    PixelTexGenQRoundSgix,
    PixelTexGenQFloorSgix,
    NearestClipmapNearestSgix,
    NearestClipmapLinearSgix,
    LinearClipmapNearestSgix
}

public enum TextureMagFilter
{
    Nearest,
    Linear,
    LinearDetailSgis,
    LinearDetailAlphaSgis,
    LinearDetailColorSgis,
    LinearSharpenSgis,
    LinearSharpenAlphaSgis,
    LinearSharpenColorSgis,
    Filter4Sgis,
    PixelTexGenQCeilingSgix,
    PixelTexGenQRoundSgix,
    PixelTexGenQFloorSgix
}

internal interface IBackendTexture : IDisposable
{
    public void LoadFromResult(ImageResult imageResult);
    public void Bind();
    public void SetWrapMode(TextureWrapMode wrapMode);
    public void SetMinFilter(TextureMinFilter minFilter);
    public void SetMagFilter(TextureMagFilter magFilter);
    public void SetMaxMips(int maxMips);    
    public void GenerateMipMaps();
}

public class Texture : IDisposable
{
    private IBackendTexture _backendTexture = null!;

    public TextureTarget TextureTarget { get; set; } = TextureTarget.Texture2D;
    public InternalFormat InternalFormat { get; set; } = InternalFormat.Rgba;
    public PixelFormat PixelFormat { get; set; } = PixelFormat.Rgba;
    public PixelType PixelType { get; set; } = PixelType.UnsignedByte;

    public TextureWrapMode WrapMode { get; set; } = TextureWrapMode.Repeat;
    public TextureMinFilter MinFilter { get; set; } = TextureMinFilter.LinearMipmapLinear;
    public TextureMagFilter MagFilter { get; set; } = TextureMagFilter.Linear;

    public Texture()
    {
        switch (Game.Instance!.Graphics.GraphicsAPIs)
        {
            case GraphicsAPIs.OpenGL:
                _backendTexture = new OpenGLTexture(TextureTarget, InternalFormat, PixelFormat, PixelType, WrapMode, MinFilter, MagFilter);
                break;
            case GraphicsAPIs.D3D11:
                _backendTexture = null!;
                throw new NotImplementedException();
        }
    }

    internal IBackendTexture GetBackendTexture()
        => _backendTexture;

    internal void ApplyBackendTexture(IBackendTexture backendTexture)
        => _backendTexture = backendTexture;

    public void Dispose()
    {
        Game.Instance?.QueueDisposable(_backendTexture);

        GC.SuppressFinalize(this);
    }

    ~Texture() => Dispose();
}
