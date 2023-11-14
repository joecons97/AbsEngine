using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsEngine.Rendering;

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

        public TerrainChunkComponent(MeshRendererComponent renderer)
        {
            Renderer = renderer;
        }
    }
}
