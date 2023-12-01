using AbsEngine.Rendering.OpenGL.Buffers;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AbsEngine.Rendering.OpenGL;

internal class OpenGLMesh : IBackendMesh
{
    public uint[] Triangles { get => _triangles; set => _triangles = value; }
    public Vector4D<float>[] Colours { get => _colours; set => _colours = value; }
    public Vector3D<float>[] Positions { get => _positions; set => _positions = value; }
    public Vector3D<float>[] Normals { get => _normals; set => _normals = value; }
    public Vector2D<float>[] Uvs { get => _uvs; set => _uvs = value; }
    public Vector3D<float>[] Tangents { get => _tangents; set => _tangents = value; }

    Vector3D<float>[] _positions;
    Vector3D<float>[] _normals;
    Vector3D<float>[] _tangents;
    Vector4D<float>[] _colours;
    Vector2D<float>[] _uvs;
    uint[] _triangles;

    private BufferContainer? _vbo;
    private BufferContainer? _ebo;
    private VertexArrayContainer<float, uint>? _vao;

    private GL _gl;
    private Mesh _mesh;

    public OpenGLMesh(Mesh mesh)
    {
        _positions = Array.Empty<Vector3D<float>>();
        _normals = Array.Empty<Vector3D<float>>();
        _colours = Array.Empty<Vector4D<float>>();
        _uvs = Array.Empty<Vector2D<float>>();
        _triangles = Array.Empty<uint>();
        _tangents = Array.Empty<Vector3D<float>>();

        _gl = ((OpenGLGraphics)Game.Instance!.Graphics).Gl;
        _mesh = mesh;
    }

    public void BuildVertexBuffer<T>(Span<T> vertices) where T : unmanaged
    {
        _vbo = new BufferContainer(_gl, BufferTargetARB.ArrayBuffer);
        _vbo.Build(vertices);
    }

    public void Build()
    {
        if (_mesh.VertexAttributeDescriptors != null)
        {
            unsafe
            {
                if (_vbo == null)
                    throw new Exception("Unable to build mesh as Vertex Buffer has not been created");

                if (Triangles != null && Triangles.Length != 0)
                {
                    _ebo = new BufferContainer(_gl, BufferTargetARB.ElementArrayBuffer);
                    _ebo.Build<uint>(Triangles.ToArray());

                    _vao = new VertexArrayContainer<float, uint>(_gl, _vbo, _ebo);
                }
                else
                {
                    _vao = new VertexArrayContainer<float, uint>(_gl, _vbo);
                }

                uint index = 0;
                int offset = 0;
                uint vertexSize = (uint)_mesh.VertexAttributeDescriptors.Sum(x => x.SizeOf() * x.Dimension);

                foreach (var item in _mesh.VertexAttributeDescriptors)
                {
                    VertexAttribPointerType type = default;
                    bool normalised = false;
                    
                    switch(item.Format)
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
            }
        }
        else
        {
            if (_positions == null) return;

            var fallbackLength = Positions.Length;

            int positionsOffset = Positions.Length * 3;
            int coloursOffset = Colours == null || !Colours.Any() ? 0 : Colours.Length * 4;
            int uvsOffset = Uvs == null || !Uvs.Any() ? 0 : Uvs.Length * 2;
            int normalsOffset = Normals == null || !Normals.Any() ? 0 : Normals.Length * 3;
            int tangentsOffset = Tangents == null || !Tangents.Any() ? 0 : Tangents.Length * 3;

            float[] vertices = new float[positionsOffset + coloursOffset + uvsOffset + normalsOffset + tangentsOffset];
            Task.Run(async () =>
            {
                await Task.WhenAll(
                    Task.Run(() => {
                        for (int i = 0; i < _positions.Length; i++)
                        {
                            Vector3D<float> pos = _positions[i];
                            pos.CopyTo(vertices, i * 3);
                        }
                    }),
                    Task.Run(() => {
                        for (int i = 0; i < _colours.Length; i++)
                        {
                            Vector4D<float> col = _colours[i];
                            col.CopyTo(vertices, positionsOffset + (i * 4));
                        }
                    }),
                    Task.Run(() => {
                        for (int i = 0; i < _uvs.Length; i++)
                        {
                            Vector2D<float> uv = _uvs[i];
                            uv.CopyTo(vertices, positionsOffset + coloursOffset + (i * 2));
                        }
                    }),
                    Task.Run(() => {
                        for (int i = 0; i < _normals.Length; i++)
                        {
                            Vector3D<float> normal = _normals[i];
                            normal.CopyTo(vertices, positionsOffset + coloursOffset + uvsOffset + (i * 3));
                        }
                    }),
                    Task.Run(() => {
                        for (int i = 0; i < _tangents.Length; i++)
                        {
                            Vector3D<float> tangent = _tangents[i];
                            tangent.CopyTo(vertices, positionsOffset + coloursOffset + uvsOffset + normalsOffset + (i * 3));
                        }
                    })
                );
            }).Wait();

            BuildVertexBuffer<float>(vertices);

            if (_vbo == null)
                throw new Exception("Unable to build mesh as Vertex Buffer has not been created");

            if (Triangles != null && Triangles.Length != 0)
            {
                _ebo = new BufferContainer(_gl, BufferTargetARB.ElementArrayBuffer);
                _ebo.Build<uint>(Triangles.ToArray());

                _vao = new VertexArrayContainer<float, uint>(_gl, _vbo, _ebo);
            }
            else
            {
                _vao = new VertexArrayContainer<float, uint>(_gl, _vbo);
            }

            _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 0, 0);

            if (_colours.Length > 0)
                _vao.VertexAttributePointer(1, 4, VertexAttribPointerType.Float, 0, positionsOffset);

            if (_uvs.Length > 0)
                _vao.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, 0, positionsOffset + coloursOffset);

            if (_normals.Length > 0)
                _vao.VertexAttributePointer(3, 3, VertexAttribPointerType.Float, 0, positionsOffset + coloursOffset + uvsOffset);

            if (_tangents.Length > 0)
                _vao.VertexAttributePointer(4, 3, VertexAttribPointerType.Float, 0, positionsOffset + coloursOffset + uvsOffset + normalsOffset);
        }
    }

    public void Bind()
    {
        _vao?.Bind();
        _ebo?.Bind();
        _vbo?.Bind();
    }

    public void Dispose()
    {
        _vbo?.Dispose();
        _ebo?.Dispose();
        _vao?.Dispose();
    }
}
