using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using Silk.NET.Maths;

namespace AbsGameProject.Terrain
{
    public class TerrainChunkGeneratorSystem : AbsEngine.ECS.System
    {
        int radius = 15;
        float lastX;
        float lastZ;
        bool hasBeenInitialised = false;

        List<TerrainChunkComponent> pool = new List<TerrainChunkComponent>();

        public TerrainChunkGeneratorSystem(Scene scene) : base(scene)
        {
        }

        public override void Tick(float deltaTime)
        {
            var mainCam = Scene.EntityManager.GetComponents<CameraComponent>(x => x.IsMainCamera).FirstOrDefault();
            if (mainCam == null)
                return;

            var roundedX = ((int)MathF.Floor(mainCam.Entity.Transform.LocalPosition.X / TerrainChunkComponent.WIDTH)) * TerrainChunkComponent.WIDTH;
            var roundedZ = ((int)MathF.Floor(mainCam.Entity.Transform.LocalPosition.Z / TerrainChunkComponent.WIDTH)) * TerrainChunkComponent.WIDTH;

            if (hasBeenInitialised && roundedX == lastX && roundedZ == lastZ)
                return;

            lastX = roundedX;
            lastZ = roundedZ;

            hasBeenInitialised = true;
            var activeChunks = new List<TerrainChunkComponent>();

            for (int x = -(radius / 2); x < (radius / 2); x++)
            {
                for (int z = -(radius / 2); z < (radius / 2); z++)
                {
                    int xF = roundedX + (x * TerrainChunkComponent.WIDTH);
                    int zF = roundedZ + (z * TerrainChunkComponent.WIDTH);

                    var chunk = Scene.EntityManager.GetComponents<TerrainChunkComponent>(x =>
                        x.Entity.Transform.LocalPosition.X == xF && x.Entity.Transform.LocalPosition.Z == zF);

                    if (chunk.Any())
                    {
                        HandleNeighbours(chunk.First(), xF, zF);
                        activeChunks.AddRange(chunk);
                        continue;
                    }

                    TerrainChunkComponent chunkComp;

                    if (pool.Any())
                    {
                        chunkComp = pool.First();
                        pool.Remove(chunkComp);

                        chunkComp.Entity.Transform.LocalPosition = new Vector3D<float>(xF, 0, zF);
                        chunkComp.State = TerrainChunkComponent.TerrainState.None;
                        chunkComp.Renderer.Mesh = null;
                        chunkComp.Mesh = null;

                        activeChunks.Add(chunkComp);
                    }
                    else
                    {
                        var chunkEnt = Scene.EntityManager.CreateEntity();
                        chunkComp = chunkEnt.AddComponent<TerrainChunkComponent>(chunkEnt.AddComponent<MeshRendererComponent>());
                        chunkComp.State = TerrainChunkComponent.TerrainState.None;

                        chunkEnt.Transform.LocalPosition = new Vector3D<float>(xF, 0, zF);
                        activeChunks.Add(chunkComp);
                    }

                    HandleNeighbours(chunkComp, xF, zF);
                }
            }


            foreach (var chunk in Scene.EntityManager.GetComponents<TerrainChunkComponent>()
                .Except(activeChunks)
                .OrderBy(x => Vector3D.Dot(
                    mainCam.Entity.Transform.Forward,
                    x.Entity.Transform.LocalPosition - mainCam.Entity.Transform.LocalPosition)
                < 0))
            {
                if (!pool.Contains(chunk))
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

                    pool.Add(chunk);
                }
            }

            base.Tick(deltaTime);
        }

        void HandleNeighbours(TerrainChunkComponent chunkComp, int xF, int zF)
        {
            var neighbours = Scene.EntityManager
                .GetComponents<TerrainChunkComponent>(x =>
                ((int)x.Entity.Transform.LocalPosition.X == xF &&
                (int)x.Entity.Transform.LocalPosition.Z == zF + TerrainChunkComponent.WIDTH) ||

                ((int)x.Entity.Transform.LocalPosition.X == xF &&
                (int)x.Entity.Transform.LocalPosition.Z == zF - TerrainChunkComponent.WIDTH) ||

                ((int)x.Entity.Transform.LocalPosition.X == xF + TerrainChunkComponent.WIDTH &&
                (int)x.Entity.Transform.LocalPosition.Z == zF) ||

                ((int)x.Entity.Transform.LocalPosition.X == xF - TerrainChunkComponent.WIDTH &&
                (int)x.Entity.Transform.LocalPosition.Z == zF));

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
