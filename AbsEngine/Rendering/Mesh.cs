using AbsEngine.Rendering.OpenGL;
using Silk.NET.Maths;

namespace AbsEngine.Rendering;

internal interface IBackendMesh : IDisposable
{
    public uint[] Triangles { get; set; }
    public Vector4D<float>[] Colours { get; set; }
    public Vector3D<float>[] Positions { get; set; }
    public Vector3D<float>[] Normals { get; set; }
    public Vector3D<float>[] Tangents { get; set; }
    public Vector2D<float>[] Uvs { get; set; }

    public void Build();
    public void Bind();
    void BuildVertexBuffer<T>(Span<T> vertices) where T : unmanaged;
}

public class Mesh : IDisposable
{
    public uint[] Triangles { get => _backendMesh.Triangles; set => _backendMesh.Triangles = value; }
    public Vector4D<float>[] Colours { get => _backendMesh.Colours; set => _backendMesh.Colours = value; }
    public Vector3D<float>[] Positions { get => _backendMesh.Positions; set => _backendMesh.Positions = value; }
    public Vector3D<float>[] Normals { get => _backendMesh.Normals; set => _backendMesh.Normals = value; }
    public Vector3D<float>[] Tangents { get => _backendMesh.Tangents; set => _backendMesh.Tangents = value; }
    public Vector2D<float>[] Uvs { get => _backendMesh.Uvs; set => _backendMesh.Uvs = value; }
    public VertexAttributeDescriptor[]? VertexAttributeDescriptors { get; set; }
    public bool UseTriangles { get; set; } = true;
    public bool HasBeenBuilt { get; private set; }

    internal readonly IBackendMesh _backendMesh = null!;
    private int _vertexBufferDataLength = 0;

    public int VertexCount => VertexAttributeDescriptors == null ? Positions.Length : _vertexBufferDataLength;

    public Mesh()
    {
        switch (Game.Instance!.Graphics.GraphicsAPIs)
        {
            case GraphicsAPIs.OpenGL:
                _backendMesh = new OpenGLMesh(this);
                break;
            case GraphicsAPIs.D3D11:
                _backendMesh = null!;
                throw new NotImplementedException();
        }
    }

    public void SetVertexBufferLayout(VertexAttributeDescriptor[]? vertexAttributes)
    {
        VertexAttributeDescriptors = vertexAttributes;
    }

    public void SetVertexBufferData<T>(Span<T> verts) where T : unmanaged
    {
        _vertexBufferDataLength = verts.Length;
        _backendMesh.BuildVertexBuffer(verts);
    }

    public void Build()
    {
        HasBeenBuilt = true;
        _backendMesh?.Build();
    }

    public void Bind()
        => _backendMesh?.Bind();

    public void Dispose()
    {
        if(_backendMesh != null)
            Game.Instance?.QueueDisposable(_backendMesh);

        GC.SuppressFinalize(this);
    }

    ~Mesh() => Dispose();
}
