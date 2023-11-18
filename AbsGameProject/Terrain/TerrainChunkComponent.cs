using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsEngine.Rendering;
using AbsGameProject.Blocks;
using Silk.NET.Maths;

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
            MeshConstructed,
            MeshGenerated
        }

        public TerrainState State { get; set; } = TerrainState.None;
        public ushort[,,]? VoxelData { get; set; }
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

        public ushort GetBlockId(int x, int y, int z)
        {
            if (VoxelData == null)
                return 0;

            if (x <= -1)
            {
                if (LeftNeighbour != null)
                    return LeftNeighbour.GetBlockId(WIDTH + x, y, z);

                return 0;
            }

            if (x >= WIDTH)
            {
                if (RightNeighbour != null)
                    return RightNeighbour.GetBlockId(x - WIDTH, y, z);

                return 0;
            }

            if (z <= -1)
            {
                if (SouthNeighbour != null)
                    return SouthNeighbour.GetBlockId(x, y, WIDTH + z);

                return 0;
            }

            if (z >= WIDTH)
            {
                if (NorthNeighbour != null)
                    return NorthNeighbour.GetBlockId(x, y, z - WIDTH);

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

        public void RebuildMesh()
         {
            if (_updatesSinceLastRebuild.Any(x => x.X >= WIDTH - 16))
                RightNeighbour?.RebuildMesh();

            if (_updatesSinceLastRebuild.Any(x => x.X <= 0))
                LeftNeighbour?.RebuildMesh();

            if (_updatesSinceLastRebuild.Any(x => x.Z >= WIDTH - 16))
                NorthNeighbour?.RebuildMesh();

            if (_updatesSinceLastRebuild.Any(x => x.Z <= 0))
                SouthNeighbour?.RebuildMesh();

            _updatesSinceLastRebuild.Clear();

            State = TerrainState.NoiseGenerated;
        }

        public TerrainChunkComponent(MeshRendererComponent renderer)
        {
            Renderer = renderer;
        }
    }
}
