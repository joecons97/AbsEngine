using AbsEngine.ECS;
using Silk.NET.Maths;

namespace AbsGameProject.Terrain
{
    public class TerrainLightmapGenerator : AsyncComponentSystem<TerrainChunkComponent>
    {
        protected override Func<TerrainChunkComponent, bool>? Predicate =>
            (x) => x.State == TerrainChunkComponent.TerrainState.NoiseGenerated && x.HasAllNeighbours;

        protected override int MaxIterationsPerFrame => 1;

        private readonly Vector3D<float>[] _directions = new Vector3D<float>[]
        {
            new Vector3D<float>(0,0,1),
            new Vector3D<float>(0,0,-1),
            new Vector3D<float>(0,1,0),
            new Vector3D<float>(0,-1,0),
            new Vector3D<float>(1,0,0),
            new Vector3D<float>(-1,0,0),
        };

        public TerrainLightmapGenerator(Scene scene) : base(scene)
        {
        }

        public override async Task OnTickAsync(TerrainChunkComponent component, float deltaTime)
        {
            if (component.VoxelData == null || component.Lightmap == null)
                return;

            Queue<Vector3D<float>> toPropagate = new Queue<Vector3D<float>>();

            await Task.Run(() =>
            {
                //for (int x = 0; x < TerrainChunkComponent.WIDTH; x++)
                //{
                //    for (int z = 0; z < TerrainChunkComponent.WIDTH; z++)
                //    {
                //        toPropagate.Enqueue(new Vector3D<float>(x, TerrainChunkComponent.HEIGHT - 1, z));
                //    }
                //}

                //while (toPropagate.Any())
                //{
                //    var entry = toPropagate.Dequeue();
                //    var nextPos = entry - Vector3D<float>.UnitY;
                //    var blockId = component.GetBlockId((int)nextPos.X, (int)nextPos.Y, (int)nextPos.Z);

                //    int opacity = blockId == 0 ? 0 : 1;

                //    if (opacity == 0)
                //    {
                //        if (nextPos.X >= 0 && nextPos.X <= TerrainChunkComponent.WIDTH - 1 &&
                //                    nextPos.Z >= 0 && nextPos.Z <= TerrainChunkComponent.WIDTH - 1 &&
                //                    nextPos.Y >= 0 && nextPos.Y <= TerrainChunkComponent.HEIGHT - 1)
                //            component.Lightmap[(int)nextPos.X, (int)nextPos.Y, (int)nextPos.Z] = (byte)16;

                //        toPropagate.Enqueue(nextPos);
                //    }

                //    //foreach (var direction in _directions)
                //    //{
                //    //    var neighbourPos = entry + direction;
                //    //    int currentLevel = component.GetLightmapValue((int)neighbourPos.X, (int)neighbourPos.Y, (int)neighbourPos.Z);

                //    //    int opacity = blockId == 0 ? 0 : 1;

                //    //    if (currentLevel >= lightValue - 1)
                //    //    {
                //    //        continue;
                //    //    }

                //    //    int targetLevel = lightValue - Math.Max(1, opacity);
                //    //    if (targetLevel > currentLevel)
                //    //    {
                //    //        if (targetLevel < 0)
                //    //            targetLevel = 0;

                //    //        if (neighbourPos.X >= 0 && neighbourPos.X <= TerrainChunkComponent.WIDTH - 1 &&
                //    //            neighbourPos.Z >= 0 && neighbourPos.Z <= TerrainChunkComponent.WIDTH - 1 &&
                //    //            neighbourPos.Y >= 0 && neighbourPos.Y <= TerrainChunkComponent.HEIGHT - 1)
                //    //        {
                //    //            component.Lightmap[(int)neighbourPos.X, (int)neighbourPos.Y, (int)neighbourPos.Z] = (byte)targetLevel;

                //    //        }
                //    //        toPropagate.Enqueue(neighbourPos);
                //    //    }

                //    //}
                //}

                for (int x = 0; x < TerrainChunkComponent.WIDTH; x++)
                {
                    for (int z = 0; z < TerrainChunkComponent.WIDTH; z++)
                    {
                        int k1 = 15;
                        int i1 = TerrainChunkComponent.HEIGHT - 1;

                        while (true)
                        {
                            var blockId = component.GetBlockId((int)x, (int)i1, (int)z);
                            int j1 = blockId == 0 ? 0 : 1;
                            if (j1 == 0 && k1 != 15)
                            {
                                j1 = 1;
                            }
                            k1 -= j1;
                            if (k1 > 0)
                            {
                                component.Lightmap[x, i1, z] = (byte)k1;
                            }

                            --i1;

                            if (i1 <= 0 || k1 <= 0)
                            {
                                break;
                            }
                        }
                    }
                }
            });

            //Debug.WriteLine(toPropagate.Count);
            component.State = TerrainChunkComponent.TerrainState.LightmapGenerated;
        }
    }
}
