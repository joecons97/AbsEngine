using AbsEngine.ECS.Components;
using AbsEngine.Exceptions;
using AbsEngine.Physics;
using AbsEngine.Rendering.OpenGL;
using AbsEngine.Rendering.OpenGL.Buffers;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System;
using System.Runtime.InteropServices;

namespace AbsEngine.Rendering.RenderCommand
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DrawArraysIndirectCommand
    {
        public uint count;
        public uint instanceCount;
        public uint firstVertex;
        public uint baseInstance;
    }

    internal class MultiDrawRenderCommand : IRenderCommand
    {
        public Material Material { get; set; }
        public Matrix4X4<float>[] WorldMatricies { get; set; }
        public DrawBuffer DrawBuffer { get; set; }
        public DrawArraysIndirectCommand[] Commands { get; }

        private uint? _drawCommandBuffer;
        private uint? _transformsBuffer;

        public int RenderQueuePosition => Material.Shader._backendShader.GetRenderQueuePosition();

        public MultiDrawRenderCommand(DrawBuffer drawBuffer, DrawArraysIndirectCommand[] commands, Material material, Matrix4X4<float>[] worldMatricies)
        {
            Material = material;
            WorldMatricies = worldMatricies;
            Commands = commands;
            DrawBuffer = drawBuffer;

            if (Commands.Length != WorldMatricies.Length)
                throw new Exception($"Expected WorldMatricies array length to equal Commands array length ({Commands.Length}) but received {WorldMatricies.Length}");
        }

        public void Render(IGraphics graphics, CameraComponent camera, RenderTexture target)
        {
            switch (graphics.GraphicsAPIs)
            {
                case GraphicsAPIs.OpenGL:
                    RenderOpenGL(graphics, camera, target);
                    break;
                case GraphicsAPIs.D3D11:
                    throw new NotImplementedException();
            }
        }

        unsafe void RenderOpenGL(IGraphics graphics, CameraComponent camera, RenderTexture target)
        {
            if (Game.Instance == null)
                throw new GameInstanceException();

            var gl = graphics as OpenGLGraphics;
            if (gl == null)
                throw new GraphicsApiException();

            var openGl = ((OpenGLGraphics)Game.Instance.Graphics).Gl;

            if(_transformsBuffer == null)
            {
                _transformsBuffer = openGl.CreateBuffer();
                openGl.BindBuffer(GLEnum.ShaderStorageBuffer, _transformsBuffer.Value);
                fixed (void* d = WorldMatricies)
                {
                    openGl.BufferData(GLEnum.ShaderStorageBuffer, (uint)(sizeof(Matrix4X4<float>) * WorldMatricies.Length), d, BufferUsageARB.StaticDraw);
                }
                openGl.BindBuffer(GLEnum.ShaderStorageBuffer, 0);
            }

            if (_drawCommandBuffer == null)
            {
                _drawCommandBuffer = openGl.CreateBuffer();
                openGl.BindBuffer(GLEnum.DrawIndirectBuffer, _drawCommandBuffer.Value);
                fixed (void* d = Commands)
                {
                    openGl.BufferData(GLEnum.DrawIndirectBuffer, (uint)(sizeof(DrawArraysIndirectCommand) * Commands.Length), d, BufferUsageARB.StaticDraw);
                }
            }
            else
            {
                openGl.BindBuffer(GLEnum.DrawIndirectBuffer, _drawCommandBuffer.Value);
            }

            Material.Bind();
            DrawBuffer.Bind();

            if (Material.Shader.IsTransparent)
            {
                Material.SetTexture("_DepthMap", target.DepthTexture);
                Material.SetTexture("_ColorMap", target.ColorTexture);
            }

            openGl.BindBufferBase(GLEnum.ShaderStorageBuffer, 3, _transformsBuffer.Value);

            var pMat = camera.GetProjectionMatrix();
            var vMat = camera.GetViewMatrix();
            var vpMat = vMat * pMat;

            Material.SetMatrix("_Vp", vpMat);
            Material.SetMatrix("_Projection", pMat);

            openGl.MultiDrawArraysIndirect(GLEnum.Triangles, null, (uint)Commands.Length, 0);

            openGl.BindBuffer(GLEnum.DrawIndirectBuffer, 0);
            openGl.BindBufferBase(GLEnum.ShaderStorageBuffer, 3, 0);
        }

        public bool ShouldCull(Frustum frustum)
            => false;
    }
}
