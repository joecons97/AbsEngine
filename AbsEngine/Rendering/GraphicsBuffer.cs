using AbsEngine.Exceptions;
using AbsEngine.Rendering.OpenGL;
using AbsEngine.Rendering.OpenGL.Buffers;
using Silk.NET.OpenGL;

namespace AbsEngine.Rendering;

public enum GraphicsBufferType
{
    Vertices,
    Triangles
}

internal interface IBackendGraphicsBuffer : IDisposable
{
    void Bind();
    void UnBind();
    void SetData<TDataType>(Span<TDataType> data) where TDataType : unmanaged;
}

public class GraphicsBuffer
{
    internal IBackendGraphicsBuffer _backendBuffer = null!;

    public GraphicsBufferType Type { get; }

    public GraphicsBuffer(GraphicsBufferType type)
    {
        Type = type;
        switch (Game.Instance!.Graphics.GraphicsAPIs)
        {
            case GraphicsAPIs.OpenGL:
                var gl = Game.Instance!.Graphics as OpenGLGraphics ?? throw new GraphicsApiException();

                var glType = type switch
                {
                    GraphicsBufferType.Triangles => BufferTargetARB.ElementArrayBuffer,
                    GraphicsBufferType.Vertices => BufferTargetARB.ArrayBuffer,
                    _ => throw new NotImplementedException(),
                };

                _backendBuffer = new OpenGLBufferContainer(gl.Gl, glType);
                break;
            case GraphicsAPIs.D3D11:
                _backendBuffer = null!;
                throw new NotImplementedException();
        }
    }

    public void Bind()
        => _backendBuffer?.Bind();

    public void UnBind()
        => _backendBuffer?.UnBind();

    public void SetData<TDataType>(Span<TDataType> data) where TDataType : unmanaged
        => _backendBuffer?.SetData(data);
}
