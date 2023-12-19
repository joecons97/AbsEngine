using AbsEngine.Exceptions;
using AbsEngine.Rendering.OpenGL;
using AbsEngine.Rendering.OpenGL.Buffers;
using Silk.NET.OpenGL;

namespace AbsEngine.Rendering;

internal interface IBackendDrawBuffer : IDisposable
{
    void Bind();
    void UnBind();
    void SetVertexAttributes(VertexAttributeDescriptor[] vertexAttributeDescriptors);
}

public class DrawBuffer
{
    internal IBackendDrawBuffer _backendBuffer = null!;

    public DrawBuffer(GraphicsBuffer vertexBuffer)
    {
        switch (Game.Instance!.Graphics.GraphicsAPIs)
        {
            case GraphicsAPIs.OpenGL:
                var gl = Game.Instance!.Graphics as OpenGLGraphics ?? throw new GraphicsApiException();

                if (vertexBuffer.Type != GraphicsBufferType.Vertices)
                    throw new Exception($"Expected vertexBuffer to be type of Vertices but it is {vertexBuffer.Type}");

                _backendBuffer = new OpenGLDrawBuffer(gl.Gl, (OpenGLBufferContainer)vertexBuffer._backendBuffer);
                break;
            case GraphicsAPIs.D3D11:
                _backendBuffer = null!;
                throw new NotImplementedException();
        }
    }

    public DrawBuffer(GraphicsBuffer vertexBuffer, GraphicsBuffer triangleBuffer)
    {
        switch (Game.Instance!.Graphics.GraphicsAPIs)
        {
            case GraphicsAPIs.OpenGL:
                var gl = Game.Instance!.Graphics as OpenGLGraphics ?? throw new GraphicsApiException();

                if (vertexBuffer.Type != GraphicsBufferType.Vertices)
                    throw new Exception($"Expected vertexBuffer to be type of Vertices but it is {vertexBuffer.Type}");

                if (triangleBuffer.Type != GraphicsBufferType.Triangles)
                    throw new Exception($"Expected triangleBuffer to be type of Triangles but it is {triangleBuffer.Type}");

                _backendBuffer = new OpenGLDrawBuffer(gl.Gl, 
                    (OpenGLBufferContainer)vertexBuffer._backendBuffer, (OpenGLBufferContainer)triangleBuffer._backendBuffer);

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

    public void SetVertexAttributes(VertexAttributeDescriptor[] vertexAttributeDescriptors)
        => _backendBuffer?.SetVertexAttributes(vertexAttributeDescriptors);
}
