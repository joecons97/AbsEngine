using AbsEngine.ECS;
using AbsGameProject.Components.Terrain;
using AbsGameProject.Models;
using System.Diagnostics;

namespace AbsGameProject.Systems.Terrain;

public class TerrainChunkBatcherRenderer : AbsEngine.ECS.System
{
    public static Queue<TerrainChunkComponent> BatchQueue { get; private set; } = new Queue<TerrainChunkComponent>();

    List<ChunkRenderJob> _renderJobs = new List<ChunkRenderJob>();

    Stopwatch watch = new Stopwatch();

    public TerrainChunkBatcherRenderer(Scene scene) : base(scene)
    {
        ChunkRenderJob.InitMaterials();
    }

    public override void Tick(float deltaTime)
    {
        if (BatchQueue.Count > 0)
        {
            //watch.Restart();
            var chunk = BatchQueue.Dequeue();
            var opaqueJob = chunk.StoredRenderJobOpaque;
            var transparentJob = chunk.StoredRenderJobTransparent;
            //watch.Stop();
            //Debug.WriteLine($"1 - Dequeue: {watch.ElapsedMilliseconds}ms");

            //watch.Restart();
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


            //watch.Stop();
            //Debug.WriteLine($"2 - Job Init: {watch.ElapsedMilliseconds}ms");

            //watch.Restart();
            //await Task.WhenAll(
            //    UpdateChunk(chunk, opaqueJob, ChunkRenderLayer.Opaque),
            //    UpdateChunk(chunk, transparentJob, ChunkRenderLayer.Transparent)
            //);
            //watch.Stop();
            //Debug.WriteLine($"3 - Update Task: {watch.ElapsedMilliseconds}ms");
            bool isValidUpdate = false;
            isValidUpdate =
                (chunk.TerrainVertices != null && chunk.TerrainVertices.Count > 0 && chunk.StoredRenderJobOpaque != null)
                ||
                (chunk.WaterVertices != null && chunk.WaterVertices.Count > 0 && chunk.StoredRenderJobTransparent != null);

            if (isValidUpdate)
            {
                //watch.Restart();
                chunk.StoredRenderJobOpaque?.UpdateBuffers();
                //watch.Stop();
                //Debug.WriteLine($"4 - Update Opaque Buffers: {watch.ElapsedMilliseconds}ms");

                //watch.Restart();
                chunk.StoredRenderJobTransparent?.UpdateBuffers();
                //watch.Stop();
                //Debug.WriteLine($"5 - Update Transparent Buffers: {watch.ElapsedMilliseconds}ms");

                chunk.State = TerrainChunkComponent.TerrainState.Done;
            }
            else
            {

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
                }
                else
                {
                    job.RemoveChunk(chunk);
                    if (job.HasSpaceFor(count))
                        job.AddChunk(chunk);
                }
                break;
        }
    }
}
