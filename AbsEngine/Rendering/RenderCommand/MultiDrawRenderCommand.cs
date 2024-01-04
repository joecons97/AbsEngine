using AbsEngine.ECS.Components;
using AbsEngine.Exceptions;
using AbsEngine.Physics;
using AbsEngine.Rendering.OpenGL;
using AbsEngine.Rendering.RenderCommand.Internal.MultiDrawRenderCommand;
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

    public class MultiDrawRenderCommand<T> : IRenderCommand where T : unmanaged
    {
        public Material Material
        {
            get => _material;
            set 
            { 
                _material = value; 
                _internalDrawCommand.SetMaterial(_material);
            }
        }

        public T[]? MaterialBuffer
        {
            get => _materialBuffer; 
            set
            {
                _materialBuffer = value;
                _internalDrawCommand.SetMaterialBufferObjects(_materialBuffer);
            }
        }
        public DrawBuffer DrawBuffer
        {
            get => _drawBuffer; 
            set
            {
                _drawBuffer = value;
                _internalDrawCommand.SetDrawBuffer(_drawBuffer);
            }
        }

        public DrawArraysIndirectCommand[] Commands
        {
            get => _commands; 
            set
            {
                _commands = value;
                _internalDrawCommand.SetDrawCommands(_commands);
            }
        }

        private IMultiDrawRenderCommand<T> _internalDrawCommand = null!;
        private Material _material;
        private T[]? _materialBuffer;
        private DrawBuffer _drawBuffer;
        private DrawArraysIndirectCommand[] _commands;

        public int RenderQueuePosition => Material.Shader._backendShader.GetRenderQueuePosition();

        public MultiDrawRenderCommand(DrawBuffer drawBuffer, DrawArraysIndirectCommand[] commands, Material material, T[] materialBuffer)
        {
            var game = Game.Instance;
            if (game == null)
                throw new GameInstanceException();

            switch (game.Graphics.GraphicsAPIs)
            {
                case GraphicsAPIs.OpenGL:
                    _internalDrawCommand = new OpenGLMultiDrawRenderCommand<T>();
                    break;
                case GraphicsAPIs.D3D11:
                    throw new NotImplementedException();
            }

            _material = material;
            _materialBuffer = materialBuffer;
            _commands = commands;
            _drawBuffer = drawBuffer;

            _internalDrawCommand.SetMaterial(_material);
            _internalDrawCommand.SetMaterialBufferObjects(_materialBuffer);
            _internalDrawCommand.SetDrawBuffer(_drawBuffer);
            _internalDrawCommand.SetDrawCommands(_commands);
        }

        public MultiDrawRenderCommand(DrawBuffer drawBuffer, DrawArraysIndirectCommand[] commands, Material material)
            : this(drawBuffer, commands, material, default!)
        {

        }


        public void Render(IGraphics graphics, CameraComponent camera, RenderTexture target)
            => _internalDrawCommand.Render(camera, target);

        public bool ShouldCull(Frustum frustum)
            => false;

        public void Dispose()
        {
            _internalDrawCommand.Dispose();
        }
    }
}
