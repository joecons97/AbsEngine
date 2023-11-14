using AbsEngine.Rendering.OpenGL.Buffers;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

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

    private BufferContainer<float>? _vbo;
    private BufferContainer<uint>? _ebo;
    private VertexArrayContainer<float, uint>? _vao;

    private GL _gl;

    public OpenGLMesh()
    {
        _positions = Array.Empty<Vector3D<float>>();
        _normals = Array.Empty<Vector3D<float>>();
        _colours = Array.Empty<Vector4D<float>>();
        _uvs = Array.Empty<Vector2D<float>>();
        _triangles = Array.Empty<uint>();
        _tangents = Array.Empty<Vector3D<float>>();

        _gl = ((OpenGLGraphics)Game.Instance!.Graphics).Gl;
    }

    public void Build()
    {
        if (_positions == null) return;

        var fallbackLength = Positions.Length;

        int positionsOffset = Positions.Length * 3;
        int coloursOffset = Colours == null || !Colours.Any() ? fallbackLength * 4 : Colours.Length * 4;
        int uvsOffset = Uvs == null || !Uvs.Any() ? fallbackLength * 2 : Uvs.Length * 2;
        int normalsOffset = Normals == null || !Normals.Any() ? fallbackLength * 3 : Normals.Length * 3;
        int tangentsOffset = Tangents == null || !Tangents.Any() ? fallbackLength * 3 : Tangents.Length * 3;

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
        
        _vbo = new BufferContainer<float>(_gl, vertices, BufferTargetARB.ArrayBuffer);

        if (Triangles != null && Triangles.Length != 0)
        {
            _ebo = new BufferContainer<uint>(_gl, Triangles.ToArray(), BufferTargetARB.ElementArrayBuffer);
            _vao = new VertexArrayContainer<float, uint>(_gl, _vbo, _ebo);
        }
        else
        {
            _vao = new VertexArrayContainer<float, uint>(_gl, _vbo);
        }

        _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 0, 0);
        _vao.VertexAttributePointer(1, 4, VertexAttribPointerType.Float, 0, positionsOffset);
        _vao.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, 0, positionsOffset + coloursOffset);
        _vao.VertexAttributePointer(3, 3, VertexAttribPointerType.Float, 0, positionsOffset + coloursOffset + uvsOffset);
        _vao.VertexAttributePointer(4, 3, VertexAttribPointerType.Float, 0, positionsOffset + coloursOffset + uvsOffset + normalsOffset);
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
