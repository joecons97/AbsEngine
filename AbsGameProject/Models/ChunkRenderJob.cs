using AbsEngine.Rendering;
using AbsEngine.Rendering.RenderCommand;
using AbsGameProject.Components.Terrain;
using AbsGameProject.Systems.Terrain;
using AbsGameProject.Textures;
using Silk.NET.Maths;

namespace AbsGameProject.Models;

public enum ChunkRenderLayer
{
    Opaque,
    Transparent
}

public class ChunkRenderJob
{
    const int MAX_CHUNKS_PER_JOB = 50;

    private static readonly VertexAttributeDescriptor[] VERTEX_ATTRIBS = new VertexAttributeDescriptor[]
    {
        new VertexAttributeDescriptor(VertexAttributeFormat.SInt8, 3),
        new VertexAttributeDescriptor(VertexAttributeFormat.UNorm8, 4),
        new VertexAttributeDescriptor(VertexAttributeFormat.Float16, 2),
    };

    private static Material MATERIAL_OPAQUE = new Material("TerrainShaderMultidraw");
    private static Material MATERIAL_TRANSPARENT = new Material("WaterShaderMultidraw");

    GraphicsBuffer _vertexBuffer;
    DrawBuffer _drawBuffer;

    List<DrawArraysIndirectCommand> _drawCommands;
    List<Matrix4X4<float>> _worldMatrices;
    List<TerrainVertex> _vertices;

    MultiDrawRenderCommand<Matrix4X4<float>> _interalDrawCommand;
    private List<TerrainChunkComponent> _chunks;

    public IReadOnlyList<Matrix4X4<float>> WorldMatrices
    {
        get
        {
            return _worldMatrices;
        }
    }
    public IReadOnlyList<DrawArraysIndirectCommand> DrawCommands
    {
        get
        {
            return _drawCommands;
        }
    }
    public IReadOnlyList<TerrainChunkComponent> Chunks { get => _chunks; }

    public ChunkRenderLayer Layer { get; }

    public ChunkRenderJob(ChunkRenderLayer layer)
    {
        _chunks = new List<TerrainChunkComponent>();

        _vertexBuffer = new GraphicsBuffer(GraphicsBufferType.Vertices);
        _drawBuffer = new DrawBuffer(_vertexBuffer);
        _drawBuffer.SetVertexAttributes(VERTEX_ATTRIBS);

        _drawCommands = new List<DrawArraysIndirectCommand>();
        _worldMatrices = new List<Matrix4X4<float>>();

        var mat = layer == ChunkRenderLayer.Opaque ? MATERIAL_OPAQUE : MATERIAL_TRANSPARENT;
        _interalDrawCommand = new MultiDrawRenderCommand<Matrix4X4<float>>(
            _drawBuffer, _drawCommands.ToArray(),
            mat, _worldMatrices.ToArray());

        _vertices = new List<TerrainVertex>();
        Layer = layer;
    }

    static ChunkRenderJob()
    {
        if (TextureAtlas.AtlasTexture != null)
        {
            MATERIAL_OPAQUE.SetTexture("uAtlas", TextureAtlas.AtlasTexture);
            MATERIAL_TRANSPARENT.SetTexture("uAtlas", TextureAtlas.AtlasTexture);
        }
    }

    void AddDrawCommand(DrawArraysIndirectCommand cmd)
    {
        _drawCommands.Add(cmd);
        _interalDrawCommand.Commands = _drawCommands.ToArray();
    }

    void RemoveDrawCommandAt(int index)
    {
        _drawCommands.RemoveAt(index);
        _interalDrawCommand.Commands = _drawCommands.ToArray();
    }

    void AddMatrix(Matrix4X4<float> matrix)
    {
        _worldMatrices.Add(matrix);
        _interalDrawCommand.MaterialBuffer = _worldMatrices.ToArray();
    }

    void RemoveMatrixAt(int index)
    {
        _worldMatrices.RemoveAt(index);
        _interalDrawCommand.MaterialBuffer = _worldMatrices.ToArray();

    }

    public void AddChunk(TerrainChunkComponent chunk)
    {
        if (!HasSpace())
            throw new Exception("ChunkRenderJob Capicity reached");

        if (chunk.TerrainVertices == null || chunk.WaterVertices == null)
            throw new Exception("Cannot apply unconstructed chunk to batch");

        var start = _vertices.Count;
        int count = 0;

        switch (Layer)
        {
            case ChunkRenderLayer.Opaque:
                if (chunk.TerrainVertices.Count == 0)
                    return;

                chunk.StoredRenderJobOpaque = this;
                
                count = chunk.TerrainVertices.Count;
                _vertices.AddRange(chunk.TerrainVertices);
                chunk.TerrainVertices.Clear();
                break;
            case ChunkRenderLayer.Transparent:
                if (chunk.WaterVertices.Count == 0)
                    return;

                chunk.StoredRenderJobTransparent = this;

                count = chunk.WaterVertices.Count;
                _vertices.AddRange(chunk.WaterVertices);
                chunk.WaterVertices.Clear();
                break;
        }

        var cmd = new DrawArraysIndirectCommand()
        {
            firstVertex = (uint)start,
            instanceCount = 1,
            count = (uint)count
        };

        _chunks.Add(chunk);
        _vertexBuffer.SetData<TerrainVertex>(_vertices.ToArray());

        AddDrawCommand(cmd);

        AddMatrix(chunk.Entity.Transform.WorldMatrix);
    }

    public void RemoveChunk(TerrainChunkComponent chunk)
    {
        var index = _chunks.IndexOf(chunk);

        if (index == -1)
            throw new Exception("Chunk does not exist in batch");

        var chunkCmd = _drawCommands[index];
        for (int i = index; i < _drawCommands.Count; i++)
        {
            var cmd = _drawCommands[i];
            cmd.firstVertex -= chunkCmd.count;
            _drawCommands[i] = cmd;
        }

        _vertices.RemoveRange((int)chunkCmd.firstVertex, (int)chunkCmd.count);

        _vertexBuffer.SetData<TerrainVertex>(_vertices.ToArray());

        RemoveDrawCommandAt(index);
        RemoveMatrixAt(index);
        _chunks.RemoveAt(index);

        switch (Layer)
        {
            case ChunkRenderLayer.Opaque:
                chunk.StoredRenderJobOpaque = null;
                break;
            case ChunkRenderLayer.Transparent:
                chunk.StoredRenderJobTransparent = null;
                break;
        }
    }

    public bool HasSpace()
    {
        return _chunks.Count < MAX_CHUNKS_PER_JOB;
    }

    public void Render()
    {
        Renderer.Render(_interalDrawCommand);
    }
}
