using AbsEngine.Rendering;
using AbsEngine.Rendering.RenderCommand;
using AbsGameProject.Components.Terrain;
using AbsGameProject.Structures;
using AbsGameProject.Textures;
using Silk.NET.Maths;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AbsGameProject.Models;

public enum ChunkRenderLayer
{
    Opaque,
    Transparent
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ChunkRenderJobChunkData
{
    public int scale;

    public float p1;
    public float p2;
    public float p3;

    public Matrix4X4<float> worldMat;
}

public class ChunkRenderJob
{
    const int MAX_VERTEX_COUNT = 30_000 * 100;

    private static readonly VertexAttributeDescriptor[] VERTEX_ATTRIBS = new VertexAttributeDescriptor[]
    {
        new VertexAttributeDescriptor(VertexAttributeFormat.UInt8, 3),
        new VertexAttributeDescriptor(VertexAttributeFormat.UNorm8, 3),
        new VertexAttributeDescriptor(VertexAttributeFormat.Float16, 2),
    };

    private static Material MATERIAL_OPAQUE = new Material("TerrainShaderMultidraw");
    private static Material MATERIAL_TRANSPARENT = new Material("WaterShaderMultidraw");

    GraphicsBuffer _vertexBuffer;
    DrawBuffer _drawBuffer;

    List<DrawArraysIndirectCommand> _drawCommands;
    List<ChunkRenderJobChunkData> _chunkBufferData;
    int _vertexCount = 0;

    MultiDrawRenderCommand<ChunkRenderJobChunkData> _interalDrawCommand;
    private List<TerrainChunkComponent> _chunks;

    public IReadOnlyList<ChunkRenderJobChunkData> Data
    {
        get
        {
            return _chunkBufferData;
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
        _vertexBuffer.SetUsage(GraphicsBufferUsage.Dynamic);
        _vertexBuffer.SetSize<TerrainVertex>(MAX_VERTEX_COUNT);

        _drawBuffer = new DrawBuffer(_vertexBuffer);
        _drawBuffer.SetVertexAttributes(VERTEX_ATTRIBS);

        _drawCommands = new List<DrawArraysIndirectCommand>();
        _chunkBufferData = new List<ChunkRenderJobChunkData>();

        var mat = layer == ChunkRenderLayer.Opaque ? MATERIAL_OPAQUE : MATERIAL_TRANSPARENT;
        _interalDrawCommand = new MultiDrawRenderCommand<ChunkRenderJobChunkData>(
            _drawBuffer, _drawCommands.ToArray(),
            mat, _chunkBufferData.ToArray());

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

        _vertexCount += count;

        _chunks.Add(chunk);
        _drawCommands.Add(cmd);
        _chunkBufferData.Add(new ChunkRenderJobChunkData()
        {
            worldMat = chunk.Entity.Transform.WorldMatrix,
            scale = chunk.IsFull ? 1 : 2,
        });
    }

    public void UpdateBuffers()
    {
        if (_vertexCount == 0)
            return;

        _interalDrawCommand.MaterialBuffer = _chunkBufferData.ToArray();
        _interalDrawCommand.Commands = _drawCommands.ToArray();
    }

    public int? TryGetChunkIndex(TerrainChunkComponent chunk)
    {
        var index = _chunks.IndexOf(chunk);
        if (index == -1)
            return null;

        return index;
    }

    public void RemoveChunk(TerrainChunkComponent chunk)
    {
        var index = _chunks.IndexOf(chunk);

        if (index == -1)
        {
            Debug.WriteLine("Chunk does not exist in batch");
            return;
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

        RemoveChunk(index);
    }

    public void RemoveChunk(int chunkIndex)
    {
        var chunkCmd = _drawCommands[chunkIndex];
        var nextStart = chunkCmd.firstVertex;

        for (int i = chunkIndex + 1; i < _drawCommands.Count; i++)
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

        _vertexBuffer.WaitForUpdates();

        _drawCommands.RemoveAt(chunkIndex);
        _chunkBufferData.RemoveAt(chunkIndex);
        _chunks.RemoveAt(chunkIndex);

        _vertexCount = _drawCommands.Count > 0 ?
            (int)_drawCommands.Max(x => x.firstVertex + x.count)
            : 0;

        if (_drawCommands.Count != _chunkBufferData.Count || _chunks.Count != _drawCommands.Count)
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
