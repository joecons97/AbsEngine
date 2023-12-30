using Silk.NET.OpenGL;

namespace AbsEngine.Rendering.OpenGL.Buffers
{
    internal class OpenGLBufferContainer : IBackendGraphicsBuffer

    {
        //Our handle, buffertype and the GL instance this class will use, these are private because they have no reason to be public.
        //Most of the time you would want to abstract items to make things like this invisible.
        private uint _handle;
        private BufferTargetARB _bufferType;
        private GL _gl;

        private BufferUsageARB _glUsage = BufferUsageARB.StaticDraw;

        public unsafe OpenGLBufferContainer(GL gl, BufferTargetARB bufferType)
        {
            //Setting the gl instance and storing our buffer type.
            _gl = gl;
            _bufferType = bufferType;

            //Getting the handle, and then uploading the data to said handle.
            _handle = _gl.GenBuffer();
        }

        public void SetUsage(GraphicsBufferUsage usage)
        {
            _glUsage = usage switch
            {
                GraphicsBufferUsage.Static => BufferUsageARB.StaticDraw,
                GraphicsBufferUsage.Dynamic => BufferUsageARB.DynamicDraw,
                GraphicsBufferUsage.Stream => BufferUsageARB.StaticDraw,
                _ => throw new NotImplementedException(),
            };
        }

        public unsafe void SetData<TDataType>(Span<TDataType> data) where TDataType : unmanaged
        {
            Bind();
            _gl.BufferData<TDataType>(_bufferType, data, _glUsage);
            UnBind();
        }

        public unsafe void SetSize<TDataType>(int size) where TDataType : unmanaged
        {
            Bind();
            _gl.BufferData<TDataType>(_bufferType, (nuint)(size * sizeof(TDataType)), null, _glUsage);
            UnBind();
        }

        public unsafe void SetSubData<TDataType>(Span<TDataType> data, int offset) where TDataType : unmanaged
        {
            Bind();

            _gl.BufferSubData<TDataType>(_bufferType, offset * sizeof(TDataType), data);

            UnBind();
        }

        public void Bind()
        {
            //Binding the buffer object, with the correct buffer type.
            _gl.BindBuffer(_bufferType, _handle);
        }

        public void BindBase(uint location)
        {
            _gl.BindBufferBase(_bufferType, location, _handle);
        }

        public void UnBind()
        {
            _gl.BindBuffer(_bufferType, 0);
        }

        public void UnBindBase(uint location)
        {
            _gl.BindBufferBase(_bufferType, location, 0);
        }

        public void Dispose()
        {
            //Remember to delete our buffer.
            _gl.DeleteBuffer(_handle);
        }
    }
}
