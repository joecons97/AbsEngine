using AbsEngine.ECS;
using AbsGameProject.Components.Terrain;
using AbsGameProject.Models;

namespace AbsGameProject.Systems.Terrain;

public class TerrainChunkBatcherRenderer : AbsEngine.ECS.System
{
    public static Queue<TerrainChunkComponent> BatchQueue { get; private set; } = new Queue<TerrainChunkComponent>();

    //public static bool RequiresRebuild = true;

    //GraphicsBuffer _graphicsBuffer;
    //DrawBuffer _drawBuffer;

    //DrawArraysIndirectCommand[]? _drawCommands;
    //Matrix4X4<float>[]? _chunkMatrices;

    //private readonly VertexAttributeDescriptor[] _vertexAttributes;
    //private readonly Material _material;

    //MultiDrawRenderCommand<Matrix4X4<float>> _drawCommand;

    //Stopwatch sw = new Stopwatch();

    //private readonly List<DrawArraysIndirectCommand> _cmds = new List<DrawArraysIndirectCommand>();
    //private readonly List<TerrainVertex> _verts = new List<TerrainVertex>();
    //private readonly List<Matrix4X4<float>> _mats = new List<Matrix4X4<float>>();
    //private List<TerrainChunkComponent> _activeChunks = new List<TerrainChunkComponent>();

    //bool isBuildingLists = false;
    //bool isBuildingDrawCall = false;

    List<ChunkRenderJob> _renderJobs = new List<ChunkRenderJob>();

    public TerrainChunkBatcherRenderer(Scene scene) : base(scene)
    {
        //_material = new Material("TerrainShaderMultidraw");
        //if (TextureAtlas.AtlasTexture != null)
        //    _material.SetTexture("uAtlas", TextureAtlas.AtlasTexture);

        //_vertexAttributes = new VertexAttributeDescriptor[]
        //{
        //    new VertexAttributeDescriptor(VertexAttributeFormat.SInt8, 3),
        //    new VertexAttributeDescriptor(VertexAttributeFormat.UNorm8, 4),
        //    new VertexAttributeDescriptor(VertexAttributeFormat.Float16, 2),
        //};

        //_graphicsBuffer = new GraphicsBuffer(GraphicsBufferType.Vertices);
        //_drawBuffer = new DrawBuffer(_graphicsBuffer);
        //_drawBuffer.SetVertexAttributes(_vertexAttributes);

        //_drawCommand = new MultiDrawRenderCommand<Matrix4X4<float>>(_drawBuffer, null, _material, null);

        //new Thread(new ThreadStart(MeshThread)).Start();
    }

    //void MeshThread()
    //{
    //    while (!Game.Instance?.Window.IsClosing ?? false)
    //    {
    //        if (RequiresRebuild && _activeChunks.Any() && isBuildingLists == false)
    //        {
    //            Debug.WriteLine("1: Begin");
    //            RequiresRebuild = false;
    //            isBuildingLists = true;

    //            lock (_activeChunks)
    //            {

    //                int i = 0;

    //                sw.Restart();

    //                //It's definitely this that's slow...
    //                // |
    //                // V

    //                foreach (var chunk in _activeChunks)
    //                {
    //                    _cmds.Add(new DrawArraysIndirectCommand()
    //                    {
    //                        count = (uint)chunk.TerrainVertices!.Count,
    //                        instanceCount = 1,
    //                        firstVertex = (uint)_verts.Count
    //                    });
    //                    _verts.AddRange(chunk.TerrainVertices);
    //                    _mats.Add(chunk.Entity.Transform.WorldMatrix);

    //                    chunk.State = TerrainChunkComponent.TerrainState.Done;
    //                    chunk.IsAwaitingRebuild = false;

    //                    i++;
    //                }

    //            }
    //            _activeChunks.Clear();

    //            sw.Stop();

    //            //Debug.WriteLine($"List Concat: {sw.Elapsed.TotalMilliseconds}ms for {i + 1}");

    //            Debug.WriteLine("2: Thread Fin");
    //            isBuildingDrawCall = true;
    //            isBuildingLists = false;
    //        }
    //    }
    //}

    public override void Tick(float deltaTime)
    {
        if (BatchQueue.Count > 0)
        {
            var chunk = BatchQueue.Dequeue();
            switch (chunk.State)
            {
                case TerrainChunkComponent.TerrainState.None:
                    if (chunk.StoredRenderJobOpaque != null)
                    {
                        var job = chunk.StoredRenderJobOpaque;
                        job.RemoveChunk(chunk);
                    }

                    if (chunk.StoredRenderJobTransparent != null)
                    {
                        var job = chunk.StoredRenderJobTransparent;
                        job.RemoveChunk(chunk);
                    }
                    break;
                case TerrainChunkComponent.TerrainState.MeshConstructed:
                    if (chunk.StoredRenderJobOpaque == null)
                    {
                        var job = _renderJobs.FirstOrDefault(x => x.Layer == ChunkRenderLayer.Opaque && x.HasSpace()) 
                            ?? new ChunkRenderJob(ChunkRenderLayer.Opaque);

                        if (job.Chunks.Count == 0)
                            _renderJobs.Add(job);

                        job.AddChunk(chunk);
                    }
                    else
                    {
                        var job = chunk.StoredRenderJobOpaque;
                        job.RemoveChunk(chunk);
                        job.AddChunk(chunk);
                    }

                    if (chunk.StoredRenderJobTransparent == null)
                    {
                        var job = _renderJobs.FirstOrDefault(x => x.Layer == ChunkRenderLayer.Transparent && x.HasSpace()) 
                            ?? new ChunkRenderJob(ChunkRenderLayer.Transparent);

                        if (job.Chunks.Count == 0)
                            _renderJobs.Add(job);

                        job.AddChunk(chunk);
                    }
                    else
                    {
                        var job = chunk.StoredRenderJobTransparent;
                        job.RemoveChunk(chunk);
                        job.AddChunk(chunk);
                    }
                    chunk.State = TerrainChunkComponent.TerrainState.Done;
                    break;
            }
        }

        foreach (var job in _renderJobs)
        {
            job.Render();
        }
    }
}
