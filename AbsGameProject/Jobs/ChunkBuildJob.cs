using AbsEngine.ECS;
using AbsGameProject.Components.Terrain;
using AbsGameProject.Systems.Terrain;
using Schedulers;
using Silk.NET.Maths;

namespace AbsGameProject.Jobs
{
    public class ChunkBuildJobState
    {
        public bool IsComplete { get; set; } = false;
    }

    public class ChunkBuildJob : IJob
    {
        int radius;
        int roundedX;
        int roundedZ;

        IReadOnlyCollection<Component> chunkRefs;
        List<TerrainChunkComponent> activeChunks;
        List<TerrainChunkComponent> chunkPool;

        Scene scene;

        ChunkBuildJobState state;

        public ChunkBuildJob(int radius, int roundedX, int roundedZ, IReadOnlyCollection<Component> previousChunks,
            List<TerrainChunkComponent> activeChunks, List<TerrainChunkComponent> chunkPool, Scene scene,
            ChunkBuildJobState state)
        {
            this.radius = radius;
            this.roundedX = roundedX;
            this.roundedZ = roundedZ;
            this.chunkRefs = previousChunks;
            this.activeChunks = activeChunks;
            this.chunkPool = chunkPool;
            this.scene = scene;
            this.state = state;
        }

        public void Execute()
        {
            using (Profiler.BeginEvent("Setup to list of chunks"))
            {
                for (int x = -(radius / 2); x < radius / 2; x++)
                {
                    for (int z = -(radius / 2); z < radius / 2; z++)
                    {
                        int xF = roundedX + x * TerrainChunkComponent.WIDTH;
                        int zF = roundedZ + z * TerrainChunkComponent.WIDTH;

                        var pos = new Vector3D<float>(xF, 0, zF);

                        var chunk = chunkRefs.FirstOrDefault(x => x.Entity.Transform.LocalPosition == pos);
                        var c = chunk as TerrainChunkComponent;

                        if (c != null && c.IsPooled == false)
                        {
                            activeChunks.Add(c);

                            continue;
                        }

                        TerrainChunkComponent chunkComp;

                        if (chunkPool.Count > 0)
                        {
                            chunkComp = chunkPool.First();
                            chunkPool.Remove(chunkComp);

                            chunkComp.Entity.Transform.LocalPosition = pos;
                            chunkComp.Entity.Name = chunkComp.Entity.Transform.LocalPosition.ToString();
                            chunkComp.State = TerrainChunkComponent.TerrainState.None;
                            chunkComp.IsPooled = false;

                            activeChunks.Add(chunkComp);
                        }
                        else
                        {
                            var chunkEnt = scene.EntityManager.CreateEntity();
                            chunkComp = chunkEnt.AddComponent<TerrainChunkComponent>();
                            chunkEnt.Transform.LocalPosition = pos;
                            chunkComp.Entity.Name = chunkComp.Entity.Transform.LocalPosition.ToString();
                            chunkComp.State = TerrainChunkComponent.TerrainState.None;
                            chunkComp.IsPooled = false;

                            activeChunks.Add(chunkComp);
                        }

                        HandleNeighbours(chunkComp, (int)pos.X, (int)pos.Z);
                    }
                }
            }


            using (Profiler.BeginEvent("Update chunk pool"))
            {
                foreach (var chunk in scene.EntityManager.GetComponents<TerrainChunkComponent>()
                    .Except(activeChunks))
                {
                    if (!chunkPool.Contains(chunk))
                    {
                        if (chunk.NorthNeighbour != null)
                        {
                            chunk.NorthNeighbour.SouthNeighbour = null;
                            chunk.NorthNeighbour = null;
                        }
                        if (chunk.RightNeighbour != null)
                        {
                            chunk.RightNeighbour.LeftNeighbour = null;
                            chunk.RightNeighbour = null;
                        }
                        if (chunk.LeftNeighbour != null)
                        {
                            chunk.LeftNeighbour.RightNeighbour = null;
                            chunk.LeftNeighbour = null;
                        }
                        if (chunk.SouthNeighbour != null)
                        {
                            chunk.SouthNeighbour.NorthNeighbour = null;
                            chunk.SouthNeighbour = null;
                        }

                        chunk.State = TerrainChunkComponent.TerrainState.None;
                        chunk.IsPooled = true;
                        chunk.VoxelData = null;
                        chunk.Heightmap = null;

                        chunkPool.Add(chunk);
                        TerrainChunkBatcherRenderer.QueueChunkForBatching(chunk);
                    }
                }
            }

            state.IsComplete = true;
            GC.Collect();
        }

        void HandleNeighbours(TerrainChunkComponent chunkComp, int xF, int zF)
        {
            using (Profiler.BeginEvent($"HandleNeighbours for {chunkComp}"))
            {
                var neighbours = chunkRefs.Where(x =>
                (int)x.Entity.Transform.LocalPosition.X == xF &&
                (int)x.Entity.Transform.LocalPosition.Z == zF + TerrainChunkComponent.WIDTH ||

                (int)x.Entity.Transform.LocalPosition.X == xF &&
                (int)x.Entity.Transform.LocalPosition.Z == zF - TerrainChunkComponent.WIDTH ||

                (int)x.Entity.Transform.LocalPosition.X == xF + TerrainChunkComponent.WIDTH &&
                (int)x.Entity.Transform.LocalPosition.Z == zF ||

                (int)x.Entity.Transform.LocalPosition.X == xF - TerrainChunkComponent.WIDTH &&
                (int)x.Entity.Transform.LocalPosition.Z == zF);

                chunkComp.NorthNeighbour = neighbours
                    .FirstOrDefault(x =>
                    (int)x.Entity.Transform.LocalPosition.X == xF &&
                    (int)x.Entity.Transform.LocalPosition.Z == zF + TerrainChunkComponent.WIDTH) as TerrainChunkComponent ?? chunkComp.NorthNeighbour;

                chunkComp.SouthNeighbour = neighbours
                    .FirstOrDefault(x =>
                    (int)x.Entity.Transform.LocalPosition.X == xF &&
                    (int)x.Entity.Transform.LocalPosition.Z == zF - TerrainChunkComponent.WIDTH) as TerrainChunkComponent ?? chunkComp.SouthNeighbour;

                chunkComp.LeftNeighbour = neighbours
                    .FirstOrDefault(x =>
                    (int)x.Entity.Transform.LocalPosition.X == xF - TerrainChunkComponent.WIDTH &&
                    (int)x.Entity.Transform.LocalPosition.Z == zF) as TerrainChunkComponent ?? chunkComp.LeftNeighbour;

                chunkComp.RightNeighbour = neighbours
                    .FirstOrDefault(x =>
                    (int)x.Entity.Transform.LocalPosition.X == xF + TerrainChunkComponent.WIDTH &&
                    (int)x.Entity.Transform.LocalPosition.Z == zF) as TerrainChunkComponent ?? chunkComp.RightNeighbour;

                if (chunkComp.NorthNeighbour != null)
                {
                    chunkComp.NorthNeighbour.SouthNeighbour = chunkComp;
                }
                if (chunkComp.RightNeighbour != null)
                {
                    chunkComp.RightNeighbour.LeftNeighbour = chunkComp;
                }
                if (chunkComp.LeftNeighbour != null)
                {
                    chunkComp.LeftNeighbour.RightNeighbour = chunkComp;
                }
                if (chunkComp.SouthNeighbour != null)
                {
                    chunkComp.SouthNeighbour.NorthNeighbour = chunkComp;
                }
            }
        }
    }
}
