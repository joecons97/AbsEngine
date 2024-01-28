using AbsEngine.ECS;
using AbsEngine.Physics;
using AbsGameProject.Components.Terrain;
using AbsGameProject.Maths;
using Silk.NET.Maths;

namespace AbsGameProject.Systems.Terrain
{
    public class TerrainNoiseGeneratorSystem : ComponentSystem<TerrainChunkComponent>
    {
        protected override int MaxIterationsPerFrame => 1;

        public TerrainNoiseGeneratorSystem(Scene scene) : base(scene)
        {
        }

        public override void OnTick(TerrainChunkComponent component, float deltaTime)
        {
            if ((component.State == TerrainChunkComponent.TerrainState.None && component.IsPooled == false) == false)
                return;

            if(component.Scale != 1)
            {
                component.State = TerrainChunkComponent.TerrainState.NoiseGenerated;
                return;
            }

            _ = Task.Run(() =>
            {
                var bb = new BoundingBox(0, TerrainChunkComponent.WIDTH, 0, TerrainChunkComponent.HEIGHT, 0, TerrainChunkComponent.WIDTH);
                var maxY = 0;
                var trans = component.Entity.Transform;
                component.VoxelData = new ushort[TerrainChunkComponent.WIDTH, TerrainChunkComponent.HEIGHT, TerrainChunkComponent.WIDTH];
                component.Heightmap = new byte[TerrainChunkComponent.WIDTH, TerrainChunkComponent.WIDTH];

                for (int x = 0; x < TerrainChunkComponent.WIDTH; x++)
                {
                    for (int z = 0; z < TerrainChunkComponent.WIDTH; z++)
                    {
                        var h = Heightmap.GetHeightAt((int)(x + trans.LocalPosition.X), (int)(z + trans.LocalPosition.Z));

                        if (h > maxY)
                            maxY = (int)h;

                        component.Heightmap[x, z] = Math.Min((byte)h, (byte)TerrainChunkComponent.HEIGHT);
                        for (int y = 0; y < TerrainChunkComponent.HEIGHT - 1; y++)
                        {
                            if (y == (int)h - 1)
                            {
                                if (y <= TerrainChunkComponent.WATER_HEIGHT + 1)
                                    component.VoxelData[x, y, z] = 6;
                                else
                                    component.VoxelData[x, y, z] = 3;
                            }
                            else if (y < h - 1 && y > h - 4)
                            {
                                if (y <= TerrainChunkComponent.WATER_HEIGHT + 1)
                                    component.VoxelData[x, y, z] = 6;
                                else
                                    component.VoxelData[x, y, z] = 2;
                            }
                            else if (y < h - 4)
                            {
                                component.VoxelData[x, y, z] = 1;
                            }
                            else
                            {
                                if (y <= TerrainChunkComponent.WATER_HEIGHT)
                                    component.VoxelData[x, y, z] = 5;
                            }
                        }
                    }
                }

                maxY = Math.Min(maxY + 32, TerrainChunkComponent.HEIGHT);
                bb.Max = new Vector3D<float>(TerrainChunkComponent.WIDTH, maxY, TerrainChunkComponent.WIDTH);
                component.BoundingBox = bb;

                component.State = TerrainChunkComponent.TerrainState.NoiseGenerated;
            });
        }
    }
}

