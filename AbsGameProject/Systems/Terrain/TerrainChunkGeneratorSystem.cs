using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsEngine.Physics;
using AbsEngine.Rendering;
using AbsGameProject.Components.Terrain;
using Silk.NET.Maths;

namespace AbsGameProject.Systems.Terrain
{
    public class TerrainChunkGeneratorSystem : AbsEngine.ECS.System
    {
        readonly List<TerrainChunkComponent> CHUNK_POOL = new();
        readonly List<TerrainChunkComponent> ACTIVE_CHUNKS = new();

        const int RADIUS = 30;
        float lastX;
        float lastZ;
        bool hasBeenInitialised = false;

        List<Vector3D<float>> queuedPositions = new();

        public TerrainChunkGeneratorSystem(Scene scene) : base(scene)
        {
            var rh = RADIUS / 2;
            Shader.SetGlobalFloat("FogMaxDistance", (rh - 1) * TerrainChunkComponent.WIDTH * TerrainChunkComponent.WIDTH);
            Shader.SetGlobalFloat("FogMinDistance", (rh - 5) * TerrainChunkComponent.WIDTH * TerrainChunkComponent.WIDTH);
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

            Task.Run(() =>
            {
                for (int x = -(RADIUS / 2); x < RADIUS / 2; x++)
                {
                    for (int z = -(RADIUS / 2); z < RADIUS / 2; z++)
                    {
                        int xF = roundedX + x * TerrainChunkComponent.WIDTH;
                        int zF = roundedZ + z * TerrainChunkComponent.WIDTH;

                        var chunk = Scene.EntityManager.GetComponents<TerrainChunkComponent>(x =>
                            x.Entity.Transform.LocalPosition.X == xF && x.Entity.Transform.LocalPosition.Z == zF);

                        if (chunk.Any())
                        {
                            HandleNeighbours(chunk.First(), xF, zF);
                            ACTIVE_CHUNKS.Add(chunk.First());

                            continue;
                        }

                        queuedPositions.Add(new Vector3D<float>(xF, 0, zF));

                    }
                }
            }).Wait();

            foreach (var queuedPosition in queuedPositions
                .OrderBy(x => Vector3D.DistanceSquared(x, mainCam.Position)))
            {
                TerrainChunkComponent chunkComp;

                if (CHUNK_POOL.Any())
                {
                    chunkComp = CHUNK_POOL.First();
                    CHUNK_POOL.Remove(chunkComp);

                    chunkComp.Entity.Transform.LocalPosition = queuedPosition;
                    chunkComp.Entity.Name = chunkComp.Entity.Transform.LocalPosition.ToString();
                    chunkComp.State = TerrainChunkComponent.TerrainState.None;
                    chunkComp.VoxelData = null;
                    chunkComp.WaterVertices?.Clear();
                    chunkComp.TerrainVertices?.Clear();

                    ACTIVE_CHUNKS.Add(chunkComp);
                }
                else
                {
                    var chunkEnt = Scene.EntityManager.CreateEntity();
                    chunkComp = chunkEnt.AddComponent<TerrainChunkComponent>();
                    chunkEnt.Transform.LocalPosition = queuedPosition;
                    chunkComp.Entity.Name = chunkComp.Entity.Transform.LocalPosition.ToString();
                    chunkComp.State = TerrainChunkComponent.TerrainState.None;

                    ACTIVE_CHUNKS.Add(chunkComp);
                }

                HandleNeighbours(chunkComp, (int)queuedPosition.X, (int)queuedPosition.Z);
            }

            queuedPositions.Clear();

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

                    CHUNK_POOL.Add(chunk);
                }
            }

            base.Tick(deltaTime);
        }

        void HandleNeighbours(TerrainChunkComponent chunkComp, int xF, int zF)
        {
            var neighbours = Scene.EntityManager
                .GetComponents<TerrainChunkComponent>(x =>
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
                (int)x.Entity.Transform.LocalPosition.Z == zF + TerrainChunkComponent.WIDTH) ?? chunkComp.NorthNeighbour;

            chunkComp.SouthNeighbour = neighbours
                .FirstOrDefault(x =>
                (int)x.Entity.Transform.LocalPosition.X == xF &&
                (int)x.Entity.Transform.LocalPosition.Z == zF - TerrainChunkComponent.WIDTH) ?? chunkComp.SouthNeighbour;

            chunkComp.LeftNeighbour = neighbours
                .FirstOrDefault(x =>
                (int)x.Entity.Transform.LocalPosition.X == xF - TerrainChunkComponent.WIDTH &&
                (int)x.Entity.Transform.LocalPosition.Z == zF) ?? chunkComp.LeftNeighbour;

            chunkComp.RightNeighbour = neighbours
                .FirstOrDefault(x =>
                (int)x.Entity.Transform.LocalPosition.X == xF + TerrainChunkComponent.WIDTH &&
                (int)x.Entity.Transform.LocalPosition.Z == zF) ?? chunkComp.RightNeighbour;

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
