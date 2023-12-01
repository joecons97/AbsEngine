﻿using Silk.NET.OpenGL;
using System.ComponentModel.DataAnnotations;

namespace AbsEngine.Rendering.OpenGL.Buffers
{
    internal class BufferContainer : IDisposable
        
    {
        //Our handle, buffertype and the GL instance this class will use, these are private because they have no reason to be public.
        //Most of the time you would want to abstract items to make things like this invisible.
        private uint _handle;
        private BufferTargetARB _bufferType;
        private GL _gl;

        public unsafe BufferContainer(GL gl, BufferTargetARB bufferType) 
        {
            //Setting the gl instance and storing our buffer type.
            _gl = gl;
            _bufferType = bufferType;

            //Getting the handle, and then uploading the data to said handle.
            _handle = _gl.GenBuffer();
        }

        public unsafe void Build<TDataType>(Span<TDataType> data) where TDataType : unmanaged
        {
            Bind();
            fixed (void* d = data)
            {
                _gl.BufferData(_bufferType, (nuint)(data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
            }
            UnBind();
        }

        public void Bind()
        {
            //Binding the buffer object, with the correct buffer type.
            _gl.BindBuffer(_bufferType, _handle);
        }

        public void UnBind()
        {
            _gl.BindBuffer(_bufferType, 0);
        }

        public void Dispose()   
        {
            //Remember to delete our buffer.
            _gl.DeleteBuffer(_handle);
        }
    }
}
