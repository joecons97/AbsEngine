using AbsEngine.ECS.Components;
using AbsEngine.Exceptions;
using AbsEngine.Rendering.OpenGL;
using AbsEngine.Rendering.OpenGL.Buffers;
using Silk.NET.OpenGL;

namespace AbsEngine.Rendering.RenderCommand.Internal.MultiDrawRenderCommand;

internal class OpenGLMultiDrawRenderCommand<T> : IMultiDrawRenderCommand<T> where T : unmanaged
{
    OpenGLBufferContainer? _drawCommandsBuffer;
    OpenGLBufferContainer? _materialBuffer;
    DrawBuffer? _drawBuffer;
    Material? _material;

    int _drawCommandsCount;

    GL gl;

    public OpenGLMultiDrawRenderCommand()
    {
        var game = Game.Instance;
        if (game == null)
            throw new GameInstanceException();

        gl = ((OpenGLGraphics)game.Graphics).Gl;
    }

    public unsafe void Render(CameraComponent camera, RenderTexture target)
    {
        if (_material == null)
            throw new ArgumentException("Unable to render OpenGLMultiDrawRenderCommand as a required buffer is null", nameof(_material));

        if (_drawCommandsBuffer == null)
            throw new ArgumentException("Unable to render OpenGLMultiDrawRenderCommand as a required buffer is null", nameof(_drawCommandsBuffer));

        if (_materialBuffer == null)
            throw new ArgumentException("Unable to render OpenGLMultiDrawRenderCommand as a required buffer is null", nameof(_materialBuffer));

        if (_drawBuffer == null)
            throw new ArgumentException("Unable to render OpenGLMultiDrawRenderCommand as a required buffer is null", nameof(_drawBuffer));

        _drawCommandsBuffer.Bind();

        _material.Bind();
        _drawBuffer.Bind();

        if (_material.Shader.IsTransparent)
        {
            _material.SetTexture("_DepthMap", target.DepthTexture);
            _material.SetTexture("_ColorMap", target.ColorTexture);
        }

        _materialBuffer.BindBase(3);

        var pMat = camera.GetProjectionMatrix();
        var vMat = camera.GetViewMatrix();
        var vpMat = vMat * pMat;

        _material.SetMatrix("_Vp", vpMat);
        _material.SetMatrix("_Projection", pMat);

        gl.MultiDrawArraysIndirect(GLEnum.Triangles, null, (uint)_drawCommandsCount, 0);

        _drawCommandsBuffer.UnBind();
        _materialBuffer.UnBindBase(3);
    }

    public void SetDrawBuffer(DrawBuffer buffer)
    {
        _drawBuffer = buffer;
    }

    public void SetDrawCommands(DrawArraysIndirectCommand[] commands)
    {
        if(commands == null)
        {
            _drawCommandsBuffer?.Dispose();

            return;
        }

        if (_drawCommandsBuffer == null)
        {
            _drawCommandsBuffer = new OpenGLBufferContainer(gl, BufferTargetARB.DrawIndirectBuffer);
        }

        _drawCommandsBuffer.Bind();

        _drawCommandsBuffer.SetData<DrawArraysIndirectCommand>(commands);

        _drawCommandsBuffer.UnBind();

        _drawCommandsCount = commands.Length;
    }

    public void SetMaterial(Material mat)
    {
        _material = mat;
    }

    public void SetMaterialBufferObjects(T[]? bufferData)
    {
        if (bufferData == null)
        {
            _materialBuffer?.Dispose();

            return;
        }

        if (_materialBuffer == null)
        {
            _materialBuffer = new OpenGLBufferContainer(gl, BufferTargetARB.ShaderStorageBuffer);
        }

        _materialBuffer.Bind();

        _materialBuffer.SetData<T>(bufferData);

        _materialBuffer.UnBind();
    }

    public void Dispose()
    {
        _drawCommandsBuffer?.Dispose();
        _materialBuffer?.Dispose();
    }
}
