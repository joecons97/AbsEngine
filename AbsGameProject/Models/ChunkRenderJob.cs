using AbsEngine.Physics;
using AbsEngine.Rendering;
using AbsEngine.Rendering.RenderCommand;
using AbsGameProject.Components.Terrain;
using AbsGameProject.Structures;
using AbsGameProject.Systems.Terrain;
using AbsGameProject.Textures;
using Silk.NET.Maths;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AbsGameProject.Models;

public enum ChunkRenderLayer
{
    Opaque,
    Transparent
}

public class ChunkRenderJob
{
    const int MAX_VERTEX_COUNT = 30_000 * 100;

    private static readonly VertexAttributeDescriptor[] VERTEX_ATTRIBS = new VertexAttributeDescriptor[]
    {
        new VertexAttributeDescriptor(VertexAttributeFormat.UInt8, 3),
        new VertexAttributeDescriptor(VertexAttributeFormat.UNorm8, 4),
        new VertexAttributeDescriptor(VertexAttributeFormat.Float16, 2),
    };

    private static Material MATERIAL_OPAQUE = new Material("TerrainShaderMultidraw");
    private static Material MATERIAL_TRANSPARENT = new Material("WaterShaderMultidraw");

    GraphicsBuffer _vertexBuffer;
    DrawBuffer _drawBuffer;

    List<DrawArraysIndirectCommand> _drawCommands;
    List<Matrix4X4<float>> _worldMatrices;
    int _vertexCount = 0;

    int? _scale = null;

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

    public int Scale => _scale ?? 0;

    public ChunkRenderLayer Layer { get; }

    public ChunkRenderJob(ChunkRenderLayer layer)
    {
        _chunks = new List<TerrainChunkComponent>();

        _vertexBuffer = new GraphicsBuffer(GraphicsBufferType.Vertices);
        _vertexBuffer.SetUsage(GraphicsBufferUsage.Dynamic);
        _vertexBuffer.SetSize<TerrainVertex>(MAX_VERTEX_COUNT);

        _drawBuffer = new DrawBuffer(_vertexBuffer);
        _drawBuffer.SetVertexAttributes(VERTEX_ATTRIBS);

        _drawCommands = new List<DrawArraysIndirectCommand>();
        _worldMatrices = new List<Matrix4X4<float>>();

        var mat = layer == ChunkRenderLayer.Opaque ? MATERIAL_OPAQUE : MATERIAL_TRANSPARENT;
        _interalDrawCommand = new MultiDrawRenderCommand<Matrix4X4<float>>(
            _drawBuffer, _drawCommands.ToArray(),
            mat, _worldMatrices.ToArray());

        Layer = layer;
    }

    public static void InitMaterials()
    {
        if (TextureAtlas.AtlasTexture != null)
        {
            MATERIAL_OPAQUE.SetTexture("uAtlas", TextureAtlas.AtlasTexture);
            MATERIAL_TRANSPARENT.SetTexture("uAtlas", TextureAtlas.AtlasTexture);

            MATERIAL_OPAQUE.SetColor("uFogColour", System.Drawing.Color.CornflowerBlue);
            MATERIAL_TRANSPARENT.SetColor("uFogColour", System.Drawing.Color.CornflowerBlue);
        }
    }

    public void AddChunk(TerrainChunkComponent chunk)
    {
        if (chunk.TerrainVertices == null || chunk.WaterVertices == null)
            throw new Exception("Cannot apply unconstructed chunk to batch");

        int count = 0;

        switch (Layer)
        {
            case ChunkRenderLayer.Opaque:
                if (chunk.TerrainVertices.Count == 0)
                    throw new Exception("Cannot apply unconstructed chunk to batch");

                count = chunk.TerrainVertices.Count;
                if (!HasSpaceFor(count))
                     throw new OverflowException($"ChunkRenderJob Capacity reached " +
                        $"{_vertexCount} + {count} = {_vertexCount + count} > {MAX_VERTEX_COUNT}");

                chunk.StoredRenderJobOpaque = this;

                _vertexBuffer.SetSubData(CollectionsMarshal.AsSpan(chunk.TerrainVertices), _vertexCount);
                break;
            case ChunkRenderLayer.Transparent:
                if (chunk.WaterVertices.Count == 0)
                    throw new Exception("Cannot apply unconstructed chunk to batch");

                count = chunk.WaterVertices.Count;

                if (!HasSpaceFor(count))
                    throw new OverflowException($"ChunkRenderJob Capacity reached " +
                         $"{_vertexCount} + {count} = {_vertexCount + count} > {MAX_VERTEX_COUNT}");

                chunk.StoredRenderJobTransparent = this;

                _vertexBuffer.SetSubData(CollectionsMarshal.AsSpan(chunk.WaterVertices), _vertexCount);
                break;
        }

        var cmd = new DrawArraysIndirectCommand()
        {
            firstVertex = (uint)_vertexCount,
            instanceCount = 1,
            count = (uint)count
        };

        if (_scale == null)
            _scale = chunk.Scale;

        _vertexCount += count;

        _chunks.Add(chunk);
        _drawCommands.Add(cmd);
        _worldMatrices.Add(chunk.Entity.Transform.WorldMatrix);
    }

    public void UpdateBuffers()
    {
        if (_vertexCount == 0)
            return;

        _interalDrawCommand.MaterialBuffer = _worldMatrices.ToArray();
        _interalDrawCommand.Commands = _drawCommands.ToArray();
    }

    public void RemoveChunk(TerrainChunkComponent chunk)
    {
        var index = _chunks.IndexOf(chunk);

        if (index == -1)
        {
            Debug.WriteLine("Chunk does not exist in batch");
            return;
            //throw new Exception("Chunk does not exist in batch");
        }

        var oldMax = _drawCommands.Max(x => x.firstVertex + x.count);
        if (_vertexCount != oldMax)
        {
            throw new Exception("Somehow vertex count has got out of sync!");
        }

        switch (Layer)
        {
            case ChunkRenderLayer.Opaque:
                chunk.StoredRenderJobOpaque = null;
                break;
            case ChunkRenderLayer.Transparent:
                chunk.StoredRenderJobTransparent = null;
                break;
        }

        var chunkCmd = _drawCommands[index];
        var nextStart = chunkCmd.firstVertex;

        for (int i = index + 1; i < _drawCommands.Count; i++)
        {
            var cmd = _drawCommands[i];

            cmd.firstVertex = nextStart;

            var chunkToMove = _chunks[i];
            switch (Layer)
            {
                case ChunkRenderLayer.Opaque:
                    if (chunkToMove.TerrainVertices == null)
                        break;

                    _vertexBuffer.SetSubData(CollectionsMarshal.AsSpan(chunkToMove.TerrainVertices), (int)cmd.firstVertex);
                    cmd.count = (uint)chunkToMove.TerrainVertices.Count;
                    nextStart = cmd.firstVertex + cmd.count;

                    break;
                case ChunkRenderLayer.Transparent:
                    if (chunkToMove.WaterVertices == null)
                        break;

                    _vertexBuffer.SetSubData(CollectionsMarshal.AsSpan(chunkToMove.WaterVertices), (int)cmd.firstVertex);
                    cmd.count = (uint)chunkToMove.WaterVertices.Count;
                    nextStart = cmd.firstVertex + cmd.count;

                    break;
            }

            _drawCommands[i] = cmd;
        }

        _drawCommands.RemoveAt(index);
        _worldMatrices.RemoveAt(index);
        _chunks.RemoveAt(index);

        _vertexCount = _drawCommands.Count > 0 ?
            (int)_drawCommands.Max(x => x.firstVertex + x.count)
            : 0;

        if (_drawCommands.Count != _worldMatrices.Count || _chunks.Count != _drawCommands.Count)
        {
            throw new Exception("Somehow buffer lists have got out of sync!");
        }
    }

    public bool HasSpaceFor(int count)
    {
        return _vertexCount + count < MAX_VERTEX_COUNT;
    }

    public void Render()
    {
        Renderer.Render(_interalDrawCommand);
    }
}
