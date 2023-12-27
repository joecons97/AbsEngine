using AbsEngine.ECS;
using AbsEngine.Physics;
using AbsGameProject.Blocks;
using AbsGameProject.Components.Terrain;
using AbsGameProject.Extensions;
using AbsGameProject.Maths.Noise;
using AbsGameProject.Structures;
using Silk.NET.Maths;

namespace AbsGameProject.Systems.Terrain
{
    public class TerrainNoiseGeneratorSystem : ComponentSystem<TerrainChunkComponent>
    {
        private FastNoiseLite noise;
        private float noiseSize = 5f;
        private float amplitude = 70;

        private int octaves = 12;
        private float lacunarity = 2;
        private float persistence = 0.5f;

        private int waterHeight = 35;

        protected override Func<TerrainChunkComponent, bool>? Predicate => (x) => x.State == TerrainChunkComponent.TerrainState.None;
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
                Random random = new Random();
                List<KeyValuePair<Vector3D<float>, Decorator>> structures = new List<KeyValuePair<Vector3D<float>, Decorator>>();

                var bb = new BoundingBox(0, TerrainChunkComponent.WIDTH, 0, TerrainChunkComponent.HEIGHT, 0, TerrainChunkComponent.WIDTH);
                var maxY = 0;
                var trans = component.Entity.Transform;
                component.VoxelData = new ushort[TerrainChunkComponent.WIDTH, TerrainChunkComponent.HEIGHT, TerrainChunkComponent.WIDTH];
                component.Heightmap = new byte[TerrainChunkComponent.WIDTH, TerrainChunkComponent.WIDTH];
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

                        h = h / 2 + 1;
                        h *= amplitude;

                        if (h > maxY)
                            maxY = (int)h;

                        component.Heightmap[x, z] = (byte)h;
                        for (int y = 0; y < TerrainChunkComponent.HEIGHT; y++)
                        {
                            if (y == (int)h - 1)
                            {
                                if (y <= waterHeight + 1)
                                    component.VoxelData[x, y, z] = 6;
                                else
                                    component.VoxelData[x, y, z] = 3;
                            }
                            else if (y < h - 1 && y > h - 4)
                            {
                                if (y <= waterHeight + 1)
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
                                if (y <= waterHeight)
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

