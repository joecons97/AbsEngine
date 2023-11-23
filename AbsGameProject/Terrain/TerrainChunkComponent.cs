using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsEngine.Rendering;
using AbsGameProject.Blocks;
using Silk.NET.Maths;
using System.Diagnostics;

namespace AbsGameProject.Terrain
{
    public class TerrainChunkComponent : Component
    {
        public const int WIDTH = 16;
        public const int HEIGHT = 128;

        public enum TerrainState
        {
            None,
            NoiseGenerated,
            LightmapGenerated,
            MeshConstructed,
            MeshGenerated
        }

        public TerrainState State { get; set; } = TerrainState.None;

        public byte[,]? Heightmap { get; set; }
        public ushort[,,]? VoxelData { get; set; }
        public byte[,,]? Lightmap { get; set; }

        public Mesh? Mesh { get; set; }
        public MeshRendererComponent Renderer { get; set; }

        public bool HasAllNeighbours =>
            LeftNeighbour != null && LeftNeighbour.State != TerrainState.None &&
            RightNeighbour != null && RightNeighbour.State != TerrainState.None &&
            NorthNeighbour != null && NorthNeighbour.State != TerrainState.None &&
            SouthNeighbour != null && SouthNeighbour.State != TerrainState.None;

        public TerrainChunkComponent? LeftNeighbour;
        public TerrainChunkComponent? RightNeighbour;
        public TerrainChunkComponent? NorthNeighbour;
        public TerrainChunkComponent? SouthNeighbour;

        private List<Vector3D<float>> _updatesSinceLastRebuild = new List<Vector3D<float>>();

        public void ResetLightmap()
        {
            Lightmap = new byte[TerrainChunkComponent.WIDTH, TerrainChunkComponent.HEIGHT, TerrainChunkComponent.WIDTH];
            //for(int x = 0; x < TerrainChunkComponent.WIDTH; x++)
            //{
            //    for(int z = 0; z < TerrainChunkComponent.WIDTH; z++)
            //    {
            //        for (int y = 0; y < TerrainChunkComponent.HEIGHT; y++)
            //        {
            //            Lightmap[x, y, z] = 16;
            //        }
            //    }
            //}
        }

        public ushort GetBlockId(int x, int y, int z)
        {
            if (VoxelData == null)
                return 0;

            if (x <= -1)
            {
                if (LeftNeighbour != null)
                {
                    if (LeftNeighbour.VoxelData == null)
                        Debug.WriteLine("LeftNeighbour VoxelData is null!", "Warning");

                    return LeftNeighbour.GetBlockId(WIDTH + x, y, z);
                }

                return 0;
            }

            if (x >= WIDTH)
            {
                if (RightNeighbour != null)
                {
                    if (RightNeighbour.VoxelData == null)
                        Debug.WriteLine("RightNeighbour VoxelData is null!", "Warning");

                    return RightNeighbour.GetBlockId(x - WIDTH, y, z);
                }

                return 0;
            }

            if (z <= -1)
            {
                if (SouthNeighbour != null)
                {
                    if (SouthNeighbour.VoxelData == null)
                        Debug.WriteLine("SouthNeighbour VoxelData is null!", "Warning");

                    return SouthNeighbour.GetBlockId(x, y, WIDTH + z);
                }

                return 0;
            }

            if (z >= WIDTH)
            {
                if (NorthNeighbour != null)
                {
                    if (NorthNeighbour.VoxelData == null)
                        Debug.WriteLine("NorthNeighbour VoxelData is null!", "Warning");

                    return NorthNeighbour.GetBlockId(x, y, z - WIDTH);
                }

                return 0;
            }

            if (y < 0 || y > HEIGHT - 1)
                return 0;

            return VoxelData[x, y, z];
        }

        public void SetBlock(int x, int y, int z, Block? block, bool isRecursed = false)
        {
            if (VoxelData == null)
                return;

            if (isRecursed == false)
            {
                var vec3 = new Vector3D<float>(x, y, z);
                if (_updatesSinceLastRebuild.Contains(vec3) == false)
                    _updatesSinceLastRebuild.Add(vec3);
            }

            if (x <= -1)
            {
                if (LeftNeighbour != null)
                    LeftNeighbour.SetBlock(WIDTH + x, y, z, block, true);

                return;
            }

            if (x >= WIDTH)
            {
                if (RightNeighbour != null)
                    RightNeighbour.SetBlock(x - WIDTH, y, z, block, true);

                return;
            }

            if (z <= -1)
            {
                if (SouthNeighbour != null)
                    SouthNeighbour.SetBlock(x, y, WIDTH + z, block, true);

                return;
            }

            if (z >= WIDTH)
            {
                if (NorthNeighbour != null)
                    NorthNeighbour.SetBlock(x, y, z - WIDTH, block, true);

                return;
            }

            if (y < 0 || y > HEIGHT - 1)
                return;

            VoxelData[x, y, z] = BlockRegistry.GetBlockIndex(block);
        }

        public byte GetLightmapValue(int x, int y, int z)
        {
            if (Lightmap == null)
                return 0;

            if (x <= -1)
            {
                if (LeftNeighbour != null)
                {
                    return LeftNeighbour.GetLightmapValue(WIDTH + x, y, z);
                }

                return 0;
            }

            if (x >= WIDTH)
            {
                if (RightNeighbour != null)
                {
                    return RightNeighbour.GetLightmapValue(x - WIDTH, y, z);
                }

                return 0;
            }

            if (z <= -1)
            {
                if (SouthNeighbour != null)
                {
                    return SouthNeighbour.GetLightmapValue(x, y, WIDTH + z);
                }

                return 0;
            }

            if (z >= WIDTH)
            {
                if (NorthNeighbour != null)
                {
                    return NorthNeighbour.GetLightmapValue(x, y, z - WIDTH);
                }

                return 0;
            }

            if (y < 0 || y > HEIGHT - 1)
                return 0;

            return Lightmap[x, y, z];
        }

        public byte GetHeight(int x, int z)
        {
            if (Heightmap == null)
                return 0;

            if (x <= -1)
            {
                if (LeftNeighbour != null)
                {
                    if (LeftNeighbour.Heightmap == null)
                        Debug.WriteLine("LeftNeighbour Heightmap is null!", "Warning");

                    return LeftNeighbour.GetHeight(WIDTH + x, z);
                }

                return 0;
            }

            if (x >= WIDTH)
            {
                if (RightNeighbour != null)
                {
                    if (RightNeighbour.Heightmap == null)
                        Debug.WriteLine("RightNeighbour Heightmap is null!", "Warning");

                    return RightNeighbour.GetHeight(x - WIDTH, z);
                }

                return 0;
            }

            if (z <= -1)
            {
                if (SouthNeighbour != null)
                {
                    if (SouthNeighbour.Heightmap == null)
                        Debug.WriteLine("SouthNeighbour Heightmap is null!", "Warning");

                    return SouthNeighbour.GetHeight(x, WIDTH + z);
                }

                return 0;
            }

            if (z >= WIDTH)
            {
                if (NorthNeighbour != null)
                {
                    if (NorthNeighbour.Heightmap == null)
                        Debug.WriteLine("NorthNeighbour Heightmap is null!", "Warning");

                    return NorthNeighbour.GetHeight(x, z - WIDTH);
                }

                return 0;
            }

            return Heightmap[x, z];
        }

        public async Task RebuildMeshAsync()
        {
            if (_updatesSinceLastRebuild.Any(x => x.X >= WIDTH - 16))
                RightNeighbour?.RebuildMeshAsync();

            if (_updatesSinceLastRebuild.Any(x => x.X <= 0))
                LeftNeighbour?.RebuildMeshAsync();

            if (_updatesSinceLastRebuild.Any(x => x.Z >= WIDTH - 16))
                NorthNeighbour?.RebuildMeshAsync();

            if (_updatesSinceLastRebuild.Any(x => x.Z <= 0))
                SouthNeighbour?.RebuildMeshAsync();

            _updatesSinceLastRebuild.Clear();

            while (RightNeighbour?.State != TerrainState.MeshGenerated ||
                LeftNeighbour?.State != TerrainState.MeshGenerated ||
                NorthNeighbour?.State != TerrainState.MeshGenerated ||
                SouthNeighbour?.State != TerrainState.MeshGenerated)
                await Task.Yield();

            State = TerrainState.NoiseGenerated;
        }

        public TerrainChunkComponent(MeshRendererComponent renderer)
        {
            Renderer = renderer;
        }
    }
}
