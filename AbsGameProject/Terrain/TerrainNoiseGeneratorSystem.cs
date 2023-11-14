using AbsEngine.ECS;
using AbsGameProject.Extensions;
using Silk.NET.Maths;

namespace AbsGameProject.Terrain
{
    public class TerrainNoiseGeneratorSystem : ComponentSystem<TerrainChunkComponent>
    {
        private FastNoiseLite noise;
        private float noiseSize = 4f;
        private float amplitude = 70;

        private int octaves = 8;
        private float lacunarity = 2;
        private float persistence = 0.5f;

        protected override Func<TerrainChunkComponent, bool>? Predicate => ((x) => x.State == TerrainChunkComponent.TerrainState.None);
        protected override int MaxIterationsPerFrame => 1;
        protected override bool UseParallel => true;

        public TerrainNoiseGeneratorSystem(Scene scene) : base(scene)
        {
            noise = new FastNoiseLite();
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        }

        public override void OnTick(TerrainChunkComponent component, float deltaTime)
        {
            _ = Task.Run(() =>
            {
                var trans = component.Entity.Transform;
                component.VoxelData = new ushort[TerrainChunkComponent.WIDTH, TerrainChunkComponent.HEIGHT, TerrainChunkComponent.WIDTH];
                for (int x = 0; x < TerrainChunkComponent.WIDTH; x++)
                {
                    for (int z = 0; z < TerrainChunkComponent.WIDTH; z++)
                    {
                        var h = noise.GetNoise(
                            new Vector2D<float>(x + trans.LocalPosition.X, z + trans.LocalPosition.Z),
                            noiseSize,
                            octaves,
                            persistence,
                            lacunarity);

                        h = (h / 2) + 1;
                        h *= amplitude;
                        h /= 1.75f;

                        for (int y = 0; y < TerrainChunkComponent.HEIGHT; y++)
                        {
                            if (h <= y - 1)
                            {
                                component.VoxelData[x, y, z] = 2;
                            }
                            else if(h < y)
                            {
                                component.VoxelData[x, y, z] = 3;
                            }
                            else
                                component.VoxelData[x, y, z] = 0;
                        }
                    }
                }
                component.State = TerrainChunkComponent.TerrainState.NoiseGenerated;
            });
        }
    }
}
