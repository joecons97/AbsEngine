using Silk.NET.OpenGL;

namespace AbsEngine.Rendering.OpenGL.Buffers
{
    internal class OpenGLDrawBuffer : IBackendDrawBuffer
    {
        //Our handle and the GL instance this class will use, these are private because they have no reason to be public.
        //Most of the time you would want to abstract items to make things like this invisible.
        private uint _handle;
        private GL _gl;

        public OpenGLDrawBuffer(GL gl, OpenGLBufferContainer vbo, OpenGLBufferContainer ebo)
        {
            //Setting out handle and binding the VBO and EBO to this VAO.
            _gl = gl;
            _handle = _gl.GenVertexArray();
            Bind();
            vbo.Bind();
            ebo.Bind();
        }

        public OpenGLDrawBuffer(GL gl, OpenGLBufferContainer vbo)
        {
            //Setting out handle and binding the VBO and EBO to this VAO.
            _gl = gl;
            _handle = _gl.GenVertexArray();
            Bind();
            vbo.Bind();
        }

        public unsafe void VertexAttributePointer<TVertexType, TIndexType>
            (uint index, int count, VertexAttribPointerType type, uint vertexSize, int offSet)
            where TVertexType : unmanaged
            where TIndexType : unmanaged
        {
            //Setting up a vertex attribute pointer
            _gl.VertexAttribPointer(index, count, type, false, vertexSize * (uint)sizeof(TVertexType), (void*)(offSet * sizeof(TVertexType)));
            _gl.EnableVertexAttribArray(index);
        }

        public unsafe void SetVertexAttributes(VertexAttributeDescriptor[] vertexAttributeDescriptors)
        {
            Bind();
            uint index = 0;
            int offset = 0;
            uint vertexSize = (uint)vertexAttributeDescriptors.Sum(x => x.SizeOf() * x.Dimension);

            foreach (var item in vertexAttributeDescriptors)
            {
                VertexAttribPointerType type = default;
                bool normalised = false;

                switch (item.Format)
                {
                    case VertexAttributeFormat.Float32:
                        type = VertexAttribPointerType.Float;
                        break;
                    case VertexAttributeFormat.Float16:
                        type = VertexAttribPointerType.HalfFloat;
                        break;
                    case VertexAttributeFormat.UNorm8:
                        type = VertexAttribPointerType.UnsignedByte;
                        normalised = true;
                        break;
                    case VertexAttributeFormat.SNorm8:
                        type = VertexAttribPointerType.Byte;
                        normalised = true;
                        break;
                    case VertexAttributeFormat.UNorm16:
                        type = VertexAttribPointerType.UnsignedShort;
                        normalised = true;
                        break;
                    case VertexAttributeFormat.SNorm16:
                        type = VertexAttribPointerType.Short;
                        normalised = true;
                        break;
                    case VertexAttributeFormat.UInt8:
                        type = VertexAttribPointerType.UnsignedByte;
                        break;
                    case VertexAttributeFormat.SInt8:
                        type = VertexAttribPointerType.Byte;
                        break;
                    case VertexAttributeFormat.UInt16:
                        type = VertexAttribPointerType.UnsignedShort;
                        break;
                    case VertexAttributeFormat.SInt16:
                        type = VertexAttribPointerType.Short;
                        break;
                    case VertexAttributeFormat.UInt32:
                        type = VertexAttribPointerType.UnsignedInt;
                        break;
                    case VertexAttributeFormat.SInt32:
                        type = VertexAttribPointerType.Int;
                        break;
                };

                _gl.EnableVertexAttribArray(index);
                _gl.VertexAttribPointer(index, item.Dimension, type, normalised, vertexSize, (void*)offset);
                index++;
                offset += item.SizeOf() * item.Dimension;
            }
            UnBind();
        }

        public void Bind()
        {
            //Binding the vertex array.
            _gl.BindVertexArray(_handle);
        }

        public void UnBind()
        {
            _gl.BindVertexArray(0);
        }

        public void Dispose()
        {
            //Remember to dispose this object so the data GPU side is cleared.
            //We dont delete the VBO and EBO here, as you can have one VBO stored under multiple VAO's.
            _gl.DeleteVertexArray(_handle);
        }
    }
}
