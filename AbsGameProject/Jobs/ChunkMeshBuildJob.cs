using AbsGameProject.Blocks;
using AbsGameProject.Components.Terrain;
using AbsGameProject.Models.Meshing;
using AbsGameProject.Structures;
using Schedulers;
using Silk.NET.Maths;

namespace AbsGameProject.Jobs
{
    public class ChunkMeshBuildJob : IJob
    {
        public TerrainChunkComponent component;
        bool[] visitedLocations = new bool[TerrainChunkComponent.WIDTH * TerrainChunkComponent.WIDTH * TerrainChunkComponent.HEIGHT];

        public ChunkMeshBuildJob(TerrainChunkComponent component)
        {
            this.component = component;
        }

        public void Execute()
        {
            if (component.VoxelData == null || component.State == TerrainChunkComponent.TerrainState.MeshConstructing)
                return;

            component.IsMeshBeingConstructed = true;

            component.State = TerrainChunkComponent.TerrainState.MeshConstructing;
            if (component.TerrainVertices != null)
                component.TerrainVertices.Clear();
            else
                component.TerrainVertices = new List<TerrainVertex>();

            if (component.WaterVertices != null)
                component.WaterVertices.Clear();
            else
                component.WaterVertices = new List<TerrainVertex>();

            var vertices = new List<Vector3D<float>>();
            var colours = new List<Vector4D<float>>();
            var uvs = new List<Vector2D<float>>();

            var transparentVertices = new List<Vector3D<float>>();
            var transparentColours = new List<Vector4D<float>>();
            var transparentUvs = new List<Vector2D<float>>();

            for (int x = 0; x < TerrainChunkComponent.WIDTH; x++)
            {
                for (int z = 0; z < TerrainChunkComponent.WIDTH; z++)
                {
                    for (int y = 0; y < TerrainChunkComponent.HEIGHT; y++)
                    {
                        var state = component.GetBlockId(x, y, z) ?? 0;
                        if (state == 0)
                            continue;

                        var blockIndex = state;
                        var block = BlockRegistry.GetBlock(blockIndex);
                        if (block.Mesh == null) continue;

                        FaceDirection toCull = FaceDirection.None;

                        if (ShouldRenderFace(component, x, y, z + 1, blockIndex) == false)
                            toCull |= FaceDirection.North;

                        if (ShouldRenderFace(component, x, y, z - 1, blockIndex) == false)
                            toCull |= FaceDirection.South;

                        if (ShouldRenderFace(component, x, y + 1, z, blockIndex) == false)
                            toCull |= FaceDirection.Up;

                        if (ShouldRenderFace(component, x, y - 1, z, blockIndex) == false)
                            toCull |= FaceDirection.Down;

                        if (ShouldRenderFace(component, x + 1, y, z, blockIndex) == false)
                            toCull |= FaceDirection.West;

                        if (ShouldRenderFace(component, x - 1, y, z, blockIndex) == false)
                            toCull |= FaceDirection.East;

                        if (toCull == FaceDirection.All)
                            continue;

                        foreach (var face in block.Mesh.Faces)
                        {
                            if ((toCull & face.Key) != face.Key)
                            {
                                for (var i = 0; i < face.Value.Positions.Count; i++)
                                {
                                    var pos = face.Value.Positions[i] + new Vector3D<float>(x, y, z);

                                    var uv = face.Value.UVs[i];
                                    var col = face.Value.TintIndicies[i] == null
                                        ? new Vector3D<float>(255, 255, 255)
                                        : new Vector3D<float>(10, 204, 66);

                                    var vert = new TerrainVertex()
                                    {
                                        position = (Vector3D<byte>)pos,
                                        colour = (Vector3D<byte>)col,
                                        uv = (Vector2D<Half>)uv
                                    };

                                    if (block.Id == "water")
                                        component.WaterVertices.Add(vert);
                                    else
                                        component.TerrainVertices.Add(vert);
                                }
                            }
                        }
                    }
                }
            }

            component.State = TerrainChunkComponent.TerrainState.MeshConstructed;
            component.IsMeshBeingConstructed = false;
        }

        bool ShouldRenderFace(TerrainChunkComponent component, int x, int y, int z, int workingBlockId)
        {
            var blockId = component.GetBlockId(x, y, z);
            if (blockId == null)
                return false;

            if (blockId == 0)
                return true;

            var block = BlockRegistry.GetBlock(blockId.Value);
            if (block.IsTransparent)
            {
                if (blockId != workingBlockId)
                    return true;

                if (block.TransparentCullSelf)
                    return false;

                return false;
            }

            return false;
        }

    }
}
