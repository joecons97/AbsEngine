using AbsEngine.ECS;
using AbsGameProject.Components.Terrain;
using AbsGameProject.Models;
using System.Diagnostics;

namespace AbsGameProject.Systems.Terrain;

public class TerrainChunkBatcherRenderer : AbsEngine.ECS.System
{
    const int MAX_PER_FRAME = 2;

    static List<TerrainChunkComponent> _batchQueue = new List<TerrainChunkComponent>();

    List<ChunkRenderJob> _renderJobs = new List<ChunkRenderJob>();
    List<Task> _tasks = new List<Task>();

    bool doDequeue = true;

    public override bool UseJobSystem => false;

    public TerrainChunkBatcherRenderer(Scene scene) : base(scene)
    {
        ChunkRenderJob.InitMaterials();
    }

    public static void QueueChunkForBatching(TerrainChunkComponent chunk)
    {
        if (chunk == null)
            return;

        lock (_batchQueue)
        {
            if (_batchQueue.Contains(chunk) == false)
                _batchQueue.Add(chunk);
        }
    }

    public override async void OnTick(float deltaTime)
    {
        doDequeue = true;

        for (int i = 0; i < MAX_PER_FRAME; i++)
        {
            doDequeue = false;
            if (_batchQueue.Count > 0)
            {
                var chunk = _batchQueue.First();
                _batchQueue.Remove(chunk);
                if (chunk == null)
                    return;

                var opaqueJob = chunk.StoredRenderJobOpaque;
                var transparentJob = chunk.StoredRenderJobTransparent;

                Task<bool>? opaqueJobTask = null;
                Task<bool>? transparentJobTask = null;

                if ((chunk.TerrainVertices != null && chunk.TerrainVertices.Count > 0)
                    || (chunk.State == TerrainChunkComponent.TerrainState.None && opaqueJob != null))
                {
                    if (opaqueJob == null)
                        opaqueJob = _renderJobs
                            .FirstOrDefault(x => x.Layer == ChunkRenderLayer.Opaque && x.HasSpaceFor(chunk.TerrainVertices!.Count))
                            ?? new ChunkRenderJob(ChunkRenderLayer.Opaque);

                    opaqueJobTask = UpdateChunk(chunk, opaqueJob, ChunkRenderLayer.Opaque);
                    _tasks.Add(opaqueJobTask);
                }

                if ((chunk.WaterVertices != null && chunk.WaterVertices.Count > 0)
                    || (chunk.State == TerrainChunkComponent.TerrainState.None && transparentJob != null))
                {
                    if (transparentJob == null)
                        transparentJob = _renderJobs
                            .FirstOrDefault(x => x.Layer == ChunkRenderLayer.Transparent && x.HasSpaceFor(chunk.WaterVertices!.Count))
                            ?? new ChunkRenderJob(ChunkRenderLayer.Transparent);

                    transparentJobTask = UpdateChunk(chunk, transparentJob, ChunkRenderLayer.Transparent);
                    _tasks.Add(transparentJobTask);
                }

                await Task.WhenAll(_tasks);

                _tasks.Clear();

                if (opaqueJobTask?.Result == true)
                {
                    opaqueJob?.UpdateBuffers();
                }

                if (transparentJobTask?.Result == true)
                {
                    transparentJob?.UpdateBuffers();
                }

                bool isValidUpdate =
                        (chunk.StoredRenderJobOpaque != null || chunk.TerrainVertices == null || chunk.TerrainVertices.Count == 0)
                        &&
                        (chunk.StoredRenderJobTransparent != null || chunk.WaterVertices == null || chunk.WaterVertices.Count == 0);

                if (isValidUpdate)
                {
                    chunk.State = TerrainChunkComponent.TerrainState.Done;
                }
            }
            else
                break;
        }

        foreach (var job in _renderJobs)
        {
            job.Render();
        }
    }

    Task<bool> UpdateChunk(TerrainChunkComponent chunk, ChunkRenderJob job, ChunkRenderLayer layer)
    {
        switch (chunk.State)
        {
            case TerrainChunkComponent.TerrainState.None:
            case TerrainChunkComponent.TerrainState.NoiseGenerated:
            case TerrainChunkComponent.TerrainState.Decorated:
                if (job != null)
                {
                    job.RemoveChunk(chunk);
                    switch (layer)
                    {
                        case ChunkRenderLayer.Opaque:
                            chunk.TerrainVertices?.Clear();
                            break;
                        case ChunkRenderLayer.Transparent:
                            chunk.WaterVertices?.Clear();
                            break;
                    }

                    return Task.FromResult(true);
                }
                break;
            case TerrainChunkComponent.TerrainState.MeshConstructed:
                ChunkRenderJob? comparisonJob = null;
                int count = 0;
                switch (layer)
                {
                    case ChunkRenderLayer.Opaque:
                        if (chunk.TerrainVertices == null)
                            return Task.FromResult(false);

                        comparisonJob = chunk.StoredRenderJobOpaque;
                        count = chunk.TerrainVertices.Count;
                        break;
                    case ChunkRenderLayer.Transparent:
                        if (chunk.WaterVertices == null)
                            return Task.FromResult(false);

                        comparisonJob = chunk.StoredRenderJobTransparent;
                        count = chunk.WaterVertices.Count;
                        break;
                }
                if (comparisonJob == null)
                {
                    if (!_renderJobs.Contains(job))
                    {
                        _renderJobs.Add(job);
                        Debug.WriteLine($"Not enough batches stored, creating a new one for layer {job.Layer} ({_renderJobs.Count})");
                    }

                    job.AddChunk(chunk);

                    return Task.FromResult(true);
                }
                else
                {
                    var index = job.TryGetChunkIndex(chunk);
                    if (index == null)
                    {
                        //Try this out of sheer desperation lol 
                        job.RemoveChunk(chunk);
                        if (job.HasSpaceFor(count))
                            job.AddChunk(chunk);
                        else
                        {
                            _batchQueue.Insert(0, chunk);
                            doDequeue = true;
                        }
                    }
                    else
                    {
                        if (job.HasSpaceFor(count))
                        {
                            job.AddChunk(chunk);
                            job.RemoveChunk(index.Value);
                        }
                        else
                        {
                            job.RemoveChunk(chunk);
                            _batchQueue.Insert(0, chunk);
                            doDequeue = true;
                        }
                    }

                    return Task.FromResult(true);
                }
        }

        return Task.FromResult(false);
    }
}
