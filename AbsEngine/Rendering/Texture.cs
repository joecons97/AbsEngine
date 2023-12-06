using AbsEngine.Rendering.OpenGL;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using StbImageSharp;
using System.Numerics;

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
    public void LoadFromPixels(byte[] pixels, int width, int height);
    public void SetSize(int width, int height);
    public void Bind();
    public void SetTextureTarget(TextureTarget textureTarget);
    public void SetInternalFormat(InternalFormat internalFormat);
    public void SetPixelFormat(PixelFormat pixelFormat);
    public void SetPixelType(PixelType pixelType);
    public void SetWrapMode(TextureWrapMode wrapMode);
    public void SetMinFilter(TextureMinFilter minFilter);
    public void SetMagFilter(TextureMagFilter magFilter);
    public void SetMaxMips(int maxMips);
    public void GenerateMipMaps();
    public void Update();
}

public class Texture : IDisposable
{
    internal IBackendTexture _backendTexture = null!;

    private TextureTarget _textureTarget = TextureTarget.Texture2D;
    private InternalFormat _internalFormat = InternalFormat.Rgba;
    private PixelFormat _pixelFormat = PixelFormat.Rgba;
    private PixelType _pixelType = PixelType.UnsignedByte;
    private TextureWrapMode _wrapMode = TextureWrapMode.Repeat;
    private TextureMinFilter _minFilter = TextureMinFilter.LinearMipmapLinear;
    private TextureMagFilter _magFilter = TextureMagFilter.Linear;

    public TextureTarget TextureTarget
    {
        get => _textureTarget; set
        {
            _textureTarget = value;
            _backendTexture.SetTextureTarget(value);
        }
    }

    public InternalFormat InternalFormat
    {
        get => _internalFormat; set
        {
            _internalFormat = value;
            _backendTexture.SetInternalFormat(value);

        }
    }

    public PixelFormat PixelFormat
    {
        get => _pixelFormat; set
        {
            _pixelFormat = value;
            _backendTexture.SetPixelFormat(value);
        }
    }

    public PixelType PixelType
    {
        get => _pixelType; set
        {
            _pixelType = value;
            _backendTexture.SetPixelType(value);
        }
    }

    public TextureWrapMode WrapMode
    {
        get => _wrapMode; set
        {
            _wrapMode = value;
            _backendTexture.SetWrapMode(value);
        }
    }

    public TextureMinFilter MinFilter
    {
        get => _minFilter; set
        {
            _minFilter = value;
            _backendTexture.SetMinFilter(value);
        }
    }

    public TextureMagFilter MagFilter
    {
        get => _magFilter; set
        {
            _magFilter = value;
            _backendTexture.SetMagFilter(value);
        }
    }

    public Vector2D<int> Size { get; private set; }

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

    public Texture(int width, int height): 
        this()
    {
        SetSize(width, height);
    }

    public void SetPixels(byte[] pixels, int width, int height)
        => _backendTexture.LoadFromPixels(pixels, width, height);

    public void SetMaxMips(int maxMips)
        => _backendTexture.SetMaxMips(maxMips);

    public void SetSize(int width, int height)
    {
        Size = new Vector2D<int>(width, height);
        _backendTexture.SetSize(width, height);
    }

    public void Update()
        => _backendTexture.Update();

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
