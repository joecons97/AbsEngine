using AbsEngine.ECS;
using AbsEngine.Physics;
using AbsGameProject.Blocks;
using AbsGameProject.Maths.Physics;
using AbsGameProject.Models;
using AbsGameProject.Systems.Terrain;
using Silk.NET.Maths;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace AbsGameProject.Components.Terrain
{
    public class TerrainChunkComponent : Component
    {
        public const int WIDTH = 16;
        public const int HEIGHT = 255;

        public enum TerrainState
        {
            None,
            NoiseGenerated,
            Decorated,
            MeshConstructing,
            MeshConstructed,
            MeshGenerated,
            Done
        }

        public TerrainState State { get; set; } = TerrainState.None;

        public List<TerrainVertex>? TerrainVertices { get; set; }
        public List<TerrainVertex>? WaterVertices { get; set; }

        public bool IsAwaitingRebuild { get; set; }

        public bool IsPooled { get; set; }

        public byte[,]? Heightmap { get; set; }
        public ushort[,,]? VoxelData { get; set; }

        public BoundingBox? BoundingBox { get; set; }

        public ChunkRenderJob? StoredRenderJobOpaque { get; set; }
        public ChunkRenderJob? StoredRenderJobTransparent { get; set; }

        public Task? ConstructionTask { get; set; }

        public bool IsReadyForDecoration =>
            State == TerrainState.NoiseGenerated &&
            LeftNeighbour != null && LeftNeighbour.State >= TerrainState.NoiseGenerated &&
            RightNeighbour != null && RightNeighbour.State >= TerrainState.NoiseGenerated &&
            NorthNeighbour != null && NorthNeighbour.State >= TerrainState.NoiseGenerated &&
            SouthNeighbour != null && SouthNeighbour.State >= TerrainState.NoiseGenerated;

        public bool IsReadyForMeshGeneration =>
            ConstructionTask == null && 
            State == TerrainState.Decorated &&
            LeftNeighbour != null && LeftNeighbour.State >= TerrainState.Decorated &&
            RightNeighbour != null && RightNeighbour.State >= TerrainState.Decorated &&
            NorthNeighbour != null && NorthNeighbour.State >= TerrainState.Decorated &&
            SouthNeighbour != null && SouthNeighbour.State >= TerrainState.Decorated;

        public TerrainChunkComponent? LeftNeighbour;
        public TerrainChunkComponent? RightNeighbour;
        public TerrainChunkComponent? NorthNeighbour;
        public TerrainChunkComponent? SouthNeighbour;

        private readonly ConcurrentBag<Vector3D<float>> _updatesSinceLastRebuild = new();

        public static TerrainChunkComponent? GetAt(Scene scene, Vector3D<float> worldPos)
        {
            var chunkPos = worldPos.ToChunkPosition();

            return scene.EntityManager.GetComponents<TerrainChunkComponent>(x => x.Entity.Transform.Position == chunkPos).FirstOrDefault();
        }

        public override void OnStart()
        {
            var waterEntity = Entity.Scene.EntityManager.CreateEntity("Water");
            waterEntity.Transform.Parent = Entity.Transform;
        }

        public static bool IsPositionInBounds(Vector3D<float> pos)
        {
            return pos.X >= 0 && pos.X < WIDTH && pos.Y >= 0 && pos.Y < HEIGHT && pos.Z >= 0 && pos.Z < WIDTH;
        }

        public static bool IsPositionInBounds(int x, int y, int z)
        {
            return x >= 0 && x < WIDTH && y >= 0 && y < HEIGHT && z >= 0 && z < WIDTH;
        }

        public ushort GetBlockId(int x, int y, int z)
        {
            if (VoxelData == null)
                return 0;

            if (IsPositionInBounds(x, y, z))
            {
                return VoxelData[x, y, z];
            }
            else
            {
                if (x <= -1)
                {
                    if (LeftNeighbour != null)
                    {
                        //if (LeftNeighbour.VoxelData == null)
                        //    Debug.WriteLine("LeftNeighbour VoxelData is null!", "Warning");

                        return LeftNeighbour.GetBlockId(WIDTH + x, y, z);
                    }

                    return 0;
                }
                else if (x >= WIDTH)
                {
                    if (RightNeighbour != null)
                    {
                        //if (RightNeighbour.VoxelData == null)
                        //    Debug.WriteLine("RightNeighbour VoxelData is null!", "Warning");

                        return RightNeighbour.GetBlockId(x - WIDTH, y, z);
                    }

                    return 0;
                }

                if (z <= -1)
                {
                    if (SouthNeighbour != null)
                    {
                        //if (SouthNeighbour.VoxelData == null)
                        //    Debug.WriteLine("SouthNeighbour VoxelData is null!", "Warning");

                        return SouthNeighbour.GetBlockId(x, y, WIDTH + z);
                    }

                    return 0;
                }
                else if (z >= WIDTH)
                {
                    if (NorthNeighbour != null)
                    {
                        //if (NorthNeighbour.VoxelData == null)
                        //    Debug.WriteLine("NorthNeighbour VoxelData is null!", "Warning");

                        return NorthNeighbour.GetBlockId(x, y, z - WIDTH);
                    }

                    return 0;
                }

                if (y < 0 || y > HEIGHT - 1)
                    return 0;
            }

            return 0;
        }

        public Block GetBlock(int x, int y, int z)
        {
            var id = GetBlockId(x, y, z);
            return BlockRegistry.GetBlock(id);
        }

        public void SetBlock(int x, int y, int z, Block block, bool isRecursed = false, bool logChange = true)
        {
            if (block == null)
                throw new ArgumentNullException("block");

            if (VoxelData == null)
                return;

            if (isRecursed == false && logChange)
            {
                var vec3 = new Vector3D<float>(x, y, z);
                if (_updatesSinceLastRebuild.Contains(vec3) == false)
                    _updatesSinceLastRebuild.Add(vec3);
            }

            if (x <= -1)
            {
                if (LeftNeighbour != null)
                {
                    LeftNeighbour.SetBlock(WIDTH + x, y, z, block, true);
                    LeftNeighbour.RebuildMesh();
                }
                return;
            }

            if (x >= WIDTH)
            {
                if (RightNeighbour != null)
                {
                    RightNeighbour.SetBlock(x - WIDTH, y, z, block, true);
                    RightNeighbour.RebuildMesh();
                }
                return;
            }

            if (z <= -1)
            {
                if (SouthNeighbour != null)
                {
                    SouthNeighbour.SetBlock(x, y, WIDTH + z, block, true);
                    SouthNeighbour.RebuildMesh();
                }
                return;
            }

            if (z >= WIDTH)
            {
                if (NorthNeighbour != null)
                {
                    NorthNeighbour.SetBlock(x, y, z - WIDTH, block, true);
                    NorthNeighbour.RebuildMesh();
                }
                return;
            }

            if (y < 0 || y > HEIGHT - 1)
                return;

            VoxelData[x, y, z] = BlockRegistry.GetBlockIndex(block);

            CalculateHeightmapAtPos(x, z);
            RebuildMesh();
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

            IsAwaitingRebuild = true;
        }

        private void CalculateHeightmapAtPos(int x, int z)
        {
            if (Heightmap == null)
                throw new InvalidOperationException("Cannot CalculateHeightmapAtPos because heightmap has not been initialised!");

            for (int y = HEIGHT - 1; y > 0; y--)
            {
                var blockId = GetBlockId(x, y, z);
                var block = BlockRegistry.GetBlock(blockId);
                if (block.Opacity > 0)
                {
                    Heightmap[x, z] = (byte)y;
                    break;
                }
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) 
                return false;

            var c = (TerrainChunkComponent)obj;

            return c.Entity.Transform.LocalPosition == Entity.Transform.LocalPosition;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
