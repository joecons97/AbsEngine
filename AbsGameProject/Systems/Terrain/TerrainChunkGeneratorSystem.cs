using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsEngine.Rendering;
using AbsGameProject.Components.Terrain;
using Silk.NET.Maths;

namespace AbsGameProject.Systems.Terrain
{
    public class TerrainChunkGeneratorSystem : AbsEngine.ECS.System
    {
        readonly List<TerrainChunkComponent> CHUNK_POOL = new();
        readonly List<TerrainChunkComponent> ACTIVE_CHUNKS = new();

        const int RADIUS = 50;
        float lastX;
        float lastZ;
        bool hasBeenInitialised = false;

        IReadOnlyCollection<Component> sceneChunkListReference;

        public TerrainChunkGeneratorSystem(Scene scene) : base(scene)
        {
            var rh = RADIUS / 2;
            Shader.SetGlobalFloat("FogMaxDistance", (rh - 1) * TerrainChunkComponent.WIDTH);
            Shader.SetGlobalFloat("FogMinDistance", (rh - 5) * TerrainChunkComponent.WIDTH);

            sceneChunkListReference = Scene.EntityManager.GetComponentListReference<TerrainChunkComponent>();
        }

        public override void Tick(float deltaTime)
        {
            var mainCam = Scene.EntityManager.GetComponents<CameraComponent>(x => x.IsMainCamera).First().Entity.Transform.Parent;
            if (mainCam == null)
                return;

            var roundedX = (int)MathF.Floor(mainCam.Entity.Transform.LocalPosition.X / TerrainChunkComponent.WIDTH) * TerrainChunkComponent.WIDTH;
            var roundedZ = (int)MathF.Floor(mainCam.Entity.Transform.LocalPosition.Z / TerrainChunkComponent.WIDTH) * TerrainChunkComponent.WIDTH;

            if (hasBeenInitialised && roundedX == lastX && roundedZ == lastZ)
                return;

            lastX = roundedX;
            lastZ = roundedZ;

            ACTIVE_CHUNKS.Clear();
            hasBeenInitialised = true;

            using (Profiler.BeginEvent("Setup to list of chunks"))
            {
                for (int x = -(RADIUS / 2); x < RADIUS / 2; x++)
                {
                    for (int z = -(RADIUS / 2); z < RADIUS / 2; z++)
                    {
                        int scale = 1;
                        if (MathF.Abs(x) > 10 || MathF.Abs(z) > 10)
                            scale = 2;

                        int xF = roundedX + x * TerrainChunkComponent.WIDTH;
                        int zF = roundedZ + z * TerrainChunkComponent.WIDTH;

                        var chunk = sceneChunkListReference.FirstOrDefault(x =>
                            x.Entity.Transform.LocalPosition.X == xF && x.Entity.Transform.LocalPosition.Z == zF);

                        if (chunk != null && ((TerrainChunkComponent)chunk).Scale == scale)
                        {
                            ACTIVE_CHUNKS.Add((TerrainChunkComponent)chunk);

                            continue;
                        }

                        var pos = new Vector3D<float>(xF, 0, zF);

                        TerrainChunkComponent chunkComp;

                        if (CHUNK_POOL.Count > 0)
                        {
                            chunkComp = CHUNK_POOL.First();
                            CHUNK_POOL.Remove(chunkComp);

                            chunkComp.Entity.Transform.LocalPosition = pos;
                            chunkComp.Entity.Name = chunkComp.Entity.Transform.LocalPosition.ToString();
                            chunkComp.State = TerrainChunkComponent.TerrainState.None;
                            chunkComp.IsPooled = false;

                            chunkComp.Scale = scale;
                            ACTIVE_CHUNKS.Add(chunkComp);
                        }
                        else
                        {
                            var chunkEnt = Scene.EntityManager.CreateEntity();
                            chunkComp = chunkEnt.AddComponent<TerrainChunkComponent>();
                            chunkEnt.Transform.LocalPosition = pos;
                            
                            chunkComp.Scale = scale;
                            chunkComp.Entity.Name = chunkComp.Entity.Transform.LocalPosition.ToString();
                            chunkComp.State = TerrainChunkComponent.TerrainState.None;
                            chunkComp.IsPooled = false;

                            ACTIVE_CHUNKS.Add(chunkComp);
                        }

                        HandleNeighbours(chunkComp, (int)pos.X, (int)pos.Z);
                    }
                }
            }

            using (Profiler.BeginEvent("Update chunk pool"))
            {
                foreach (var chunk in Scene.EntityManager.GetComponents<TerrainChunkComponent>()
                .Except(ACTIVE_CHUNKS))
                {
                    if (!CHUNK_POOL.Contains(chunk))
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

                        CHUNK_POOL.Add(chunk);
                        TerrainChunkBatcherRenderer.QueueChunkForBatching(chunk);
                    }
                }
            }
            base.Tick(deltaTime);
        }

        void HandleNeighbours(TerrainChunkComponent chunkComp, int xF, int zF)
        {
            using (Profiler.BeginEvent($"HandleNeighbours for {chunkComp}"))
            {
                var neighbours = sceneChunkListReference.Where(x =>
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
