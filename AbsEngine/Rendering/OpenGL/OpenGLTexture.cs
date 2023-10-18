using Silk.NET.OpenGL;
using StbImageSharp;

namespace AbsEngine.Rendering.OpenGL;

internal class OpenGLTexture : IBackendTexture
{
    private readonly uint _handle;
    private readonly GL _gl;

    private TextureTarget _textureTarget;
    private InternalFormat _format;
    private PixelFormat _pixelFormat;
    private PixelType _pixelType;

    public OpenGLTexture(
        TextureTarget textureTarget, InternalFormat format, PixelFormat pixelFormat,
        PixelType pixelType, TextureWrapMode wrapMode, TextureMinFilter minFilter,
        TextureMagFilter magFilter)
    {
        _gl = ((OpenGLGraphics)Game.Instance!.Graphics).Gl;
        _handle = _gl.GenTexture();
        _textureTarget = textureTarget;
        _format = format;
        _pixelFormat = pixelFormat;
        _pixelType = pixelType;

        SetWrapMode(wrapMode);
        SetMinFilter(minFilter);
        SetMagFilter(magFilter);

        Bind();

        SetMaxMips(10);
    }

    public void Bind()
    {
        _gl.BindTexture(_textureTarget, _handle);
    }

    public void LoadFromResult(ImageResult imageResult)
    {
        Bind();

        _gl.TexImage2D<byte>(_textureTarget, 0, _format,
            (uint)imageResult.Width, (uint)imageResult.Height,
            0, _pixelFormat, _pixelType, imageResult.Data);

        GenerateMipMaps();
    }

    public void Dispose()
    {
        _gl.DeleteTexture(_handle);
    }

    public void SetWrapMode(TextureWrapMode wrapMode)
    {
        var finalWrapMode = wrapMode switch
        {
            TextureWrapMode.LinearMipmapLinear => Silk.NET.OpenGL.TextureWrapMode.LinearMipmapLinear,
            TextureWrapMode.Repeat => Silk.NET.OpenGL.TextureWrapMode.Repeat,
            TextureWrapMode.ClampToBorder => Silk.NET.OpenGL.TextureWrapMode.ClampToBorder,
            TextureWrapMode.ClampToBorderArb => Silk.NET.OpenGL.TextureWrapMode.ClampToBorderArb,
            TextureWrapMode.ClampToBorderNV => Silk.NET.OpenGL.TextureWrapMode.ClampToBorderNV,
            TextureWrapMode.ClampToBorderSgis => Silk.NET.OpenGL.TextureWrapMode.ClampToBorderSgis,
            TextureWrapMode.ClampToEdge => Silk.NET.OpenGL.TextureWrapMode.ClampToEdge,
            TextureWrapMode.ClampToEdgeSgis => Silk.NET.OpenGL.TextureWrapMode.ClampToEdge,
            TextureWrapMode.MirroredRepeat => Silk.NET.OpenGL.TextureWrapMode.MirroredRepeat,
            _ => throw new NotImplementedException()
        };

        _gl.TextureParameter(_handle, TextureParameterName.TextureWrapS, (int)finalWrapMode);
        _gl.TextureParameter(_handle, TextureParameterName.TextureWrapT, (int)finalWrapMode);
    }

    public void SetMinFilter(TextureMinFilter minFilter)
    {
        var finalMinFilter = minFilter switch
        {
            TextureMinFilter.Nearest => Silk.NET.OpenGL.TextureMinFilter.Nearest,
            TextureMinFilter.Linear => Silk.NET.OpenGL.TextureMinFilter.Linear,
            TextureMinFilter.NearestMipmapNearest => Silk.NET.OpenGL.TextureMinFilter.NearestMipmapNearest,
            TextureMinFilter.LinearMipmapNearest => Silk.NET.OpenGL.TextureMinFilter.LinearMipmapNearest,
            TextureMinFilter.NearestMipmapLinear => Silk.NET.OpenGL.TextureMinFilter.NearestMipmapLinear,
            TextureMinFilter.LinearMipmapLinear => Silk.NET.OpenGL.TextureMinFilter.LinearMipmapLinear,
            TextureMinFilter.Filter4Sgis => Silk.NET.OpenGL.TextureMinFilter.Filter4Sgis,
            TextureMinFilter.LinearClipmapLinearSgix => Silk.NET.OpenGL.TextureMinFilter.LinearClipmapLinearSgix,
            TextureMinFilter.PixelTexGenQCeilingSgix => Silk.NET.OpenGL.TextureMinFilter.PixelTexGenQCeilingSgix,
            TextureMinFilter.PixelTexGenQRoundSgix => Silk.NET.OpenGL.TextureMinFilter.PixelTexGenQRoundSgix,
            TextureMinFilter.PixelTexGenQFloorSgix => Silk.NET.OpenGL.TextureMinFilter.PixelTexGenQFloorSgix,
            TextureMinFilter.NearestClipmapNearestSgix => Silk.NET.OpenGL.TextureMinFilter.NearestClipmapNearestSgix,
            TextureMinFilter.NearestClipmapLinearSgix => Silk.NET.OpenGL.TextureMinFilter.NearestClipmapLinearSgix,
            TextureMinFilter.LinearClipmapNearestSgix => Silk.NET.OpenGL.TextureMinFilter.LinearClipmapNearestSgix,
            _ => throw new NotImplementedException(),
        };

        _gl.TextureParameter(_handle, TextureParameterName.TextureMinFilter, (int)finalMinFilter);
    }

    public void SetMagFilter(TextureMagFilter magFilter)
    {
        var finalMagFilter = magFilter switch
        {
            TextureMagFilter.Nearest => Silk.NET.OpenGL.TextureMagFilter.Nearest,
            TextureMagFilter.Linear => Silk.NET.OpenGL.TextureMagFilter.Linear,
            TextureMagFilter.LinearDetailSgis => Silk.NET.OpenGL.TextureMagFilter.LinearDetailSgis,
            TextureMagFilter.LinearDetailAlphaSgis => Silk.NET.OpenGL.TextureMagFilter.LinearDetailAlphaSgis,
            TextureMagFilter.LinearDetailColorSgis => Silk.NET.OpenGL.TextureMagFilter.LinearDetailColorSgis,
            TextureMagFilter.LinearSharpenSgis => Silk.NET.OpenGL.TextureMagFilter.LinearSharpenSgis,
            TextureMagFilter.LinearSharpenAlphaSgis => Silk.NET.OpenGL.TextureMagFilter.LinearSharpenAlphaSgis,
            TextureMagFilter.LinearSharpenColorSgis => Silk.NET.OpenGL.TextureMagFilter.LinearSharpenColorSgis,
            TextureMagFilter.Filter4Sgis => Silk.NET.OpenGL.TextureMagFilter.Filter4Sgis,
            TextureMagFilter.PixelTexGenQCeilingSgix => Silk.NET.OpenGL.TextureMagFilter.PixelTexGenQCeilingSgix,
            TextureMagFilter.PixelTexGenQRoundSgix => Silk.NET.OpenGL.TextureMagFilter.PixelTexGenQRoundSgix,
            TextureMagFilter.PixelTexGenQFloorSgix => Silk.NET.OpenGL.TextureMagFilter.PixelTexGenQFloorSgix,
            _ => throw new NotImplementedException(),
        };

        _gl.TextureParameter(_handle, TextureParameterName.TextureMinFilter, (int)finalMagFilter);
    }

    public void GenerateMipMaps()
    {
        _gl.GenerateMipmap(_textureTarget);
    }

    public void SetMaxMips(int maxMips)
    {
        _gl.TextureParameter(_handle, GLEnum.TextureBaseLevel, 0);
        _gl.TextureParameter(_handle, GLEnum.TextureMaxLevel, maxMips);
    }
}
