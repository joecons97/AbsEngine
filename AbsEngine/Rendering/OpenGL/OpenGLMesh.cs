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

    Vector3D<float>[] _positions;
    Vector3D<float>[] _normals;
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

        _gl = ((OpenGLGraphics)Game.Instance!.Graphics).Gl;
    }

    public unsafe void Build()
    {
        if (_positions == null) return;

        if (_colours == null)
        {
            _colours = new Vector4D<float>[_positions.Length];
        }

        if (_uvs == null)
        {
            _uvs = new Vector2D<float>[_positions.Length];
        }

        if (_normals == null)
        {
            _normals = new Vector3D<float>[_positions.Length];
        }

        if (Positions.Length != Uvs.Length)
        {
            throw new ArgumentException("the number of position and uvs must be equal");
        }

        if (Positions.Length != Normals.Length)
        {
            throw new ArgumentException("the number of position and normals must be equal");
        }

        if (Positions.Length != Colours.Length)
        {
            throw new ArgumentException("the number of position and colours must be equal");
        }

        int positionsOffset = Positions.Length * 3;
        int coloursOffset = Colours.Length * 4;
        int uvsOffset = Uvs.Length * 2;
        int normalsOffset = Normals.Length * 3;

        float[] vertices = new float[positionsOffset + coloursOffset + uvsOffset + normalsOffset];
        for (int i = 0; i < _positions.Length; i++)
        {
            Vector3D<float> pos = _positions[i];
            pos.CopyTo(vertices, i * 3);
        }

        for (int i = 0; i < _colours.Length; i++)
        {
            Vector4D<float> col = _colours[i];
            col.CopyTo(vertices, positionsOffset + (i * 4));
        }

        for (int i = 0; i < _uvs.Length; i++)
        {
            Vector2D<float> uv = _uvs[i];
            uv.CopyTo(vertices, positionsOffset + coloursOffset + (i * 2));
        }

        for (int i = 0; i < _normals.Length; i++)
        {
            Vector3D<float> normal = _normals[i];
            normal.CopyTo(vertices, positionsOffset + coloursOffset + uvsOffset + (i * 3));
        }

        _vbo = new BufferContainer<float>(_gl, vertices, BufferTargetARB.ArrayBuffer);

        if (Triangles != null)
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
