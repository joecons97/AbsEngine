using AbsEngine.ECS;
using AbsGameProject.Components.Terrain;
using AbsGameProject.Models;
using System.Diagnostics;

namespace AbsGameProject.Systems.Terrain;

public class TerrainChunkBatcherRenderer : AbsEngine.ECS.System
{
    public static Queue<TerrainChunkComponent> BatchQueue { get; private set; } = new Queue<TerrainChunkComponent>();

    List<ChunkRenderJob> _renderJobs = new List<ChunkRenderJob>();

    public TerrainChunkBatcherRenderer(Scene scene) : base(scene)
    {
        ChunkRenderJob.InitMaterials();
    }

    public override void Tick(float deltaTime)
    {
        if (BatchQueue.Count > 0)
        {
            var chunk = BatchQueue.Dequeue();
            var opaqueJob = chunk.StoredRenderJobOpaque;
            var transparentJob = chunk.StoredRenderJobTransparent;

            if (chunk.TerrainVertices != null && chunk.TerrainVertices.Count > 0)
            {
                if (opaqueJob == null)
                    opaqueJob = _renderJobs
                        .FirstOrDefault(x => x.Layer == ChunkRenderLayer.Opaque && x.HasSpaceFor(chunk.TerrainVertices.Count))
                        ?? new ChunkRenderJob(ChunkRenderLayer.Opaque);

                UpdateChunk(chunk, opaqueJob, ChunkRenderLayer.Opaque);
            }

            if (chunk.WaterVertices != null && chunk.WaterVertices.Count > 0)
            {
                if (transparentJob == null)
                    transparentJob = _renderJobs
                        .FirstOrDefault(x => x.Layer == ChunkRenderLayer.Transparent && x.HasSpaceFor(chunk.WaterVertices.Count))
                        ?? new ChunkRenderJob(ChunkRenderLayer.Transparent);

                UpdateChunk(chunk, transparentJob, ChunkRenderLayer.Transparent);
            }

            
            bool isValidUpdate = false;
            isValidUpdate =
                (chunk.TerrainVertices != null && chunk.TerrainVertices.Count > 0 && chunk.StoredRenderJobOpaque != null)
                ||
                (chunk.WaterVertices != null && chunk.WaterVertices.Count > 0 && chunk.StoredRenderJobTransparent != null);

            if (isValidUpdate)
            {
                chunk.State = TerrainChunkComponent.TerrainState.Done;
            }
        }

        foreach (var job in _renderJobs)
        {
            job.Render();
        }
    }

    void UpdateChunk(TerrainChunkComponent chunk, ChunkRenderJob job, ChunkRenderLayer layer)
    {
        switch (chunk.State)
        {
            case TerrainChunkComponent.TerrainState.None:
            case TerrainChunkComponent.TerrainState.NoiseGenerated:
            case TerrainChunkComponent.TerrainState.Decorated:
                if (job != null)
                {
                    job.RemoveChunk(chunk);
                    job.UpdateBuffers();
                }
                break;
            case TerrainChunkComponent.TerrainState.MeshConstructed:
                ChunkRenderJob? comparisonJob = null;
                int count = 0;
                switch (layer)
                {
                    case ChunkRenderLayer.Opaque:
                        if (chunk.TerrainVertices == null)
                            return;

                        comparisonJob = chunk.StoredRenderJobOpaque;
                        count = chunk.TerrainVertices.Count;
                        break;
                    case ChunkRenderLayer.Transparent:
                        if (chunk.WaterVertices == null)
                            return;

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
                    job.UpdateBuffers();
                }
                else
                {
                    job.RemoveChunk(chunk);
                    if (job.HasSpaceFor(count))
                        job.AddChunk(chunk);

                    job.UpdateBuffers();
                }
                break;
        }
    }
}
