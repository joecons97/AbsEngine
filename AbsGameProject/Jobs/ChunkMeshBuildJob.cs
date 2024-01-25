using AbsGameProject.Blocks;
using AbsGameProject.Components.Terrain;
using AbsGameProject.Models.Meshing;
using AbsGameProject.Structures;
using AbsGameProject.Systems.Terrain;
using Schedulers;
using Silk.NET.Maths;

namespace AbsGameProject.Jobs
{
    public class ChunkMeshBuildJob : IJob
    {
        public TerrainChunkComponent component;

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
            if (component.TerrainVertices == null)
                component.TerrainVertices = new List<TerrainVertex>();
            else
                component.TerrainVertices.Clear();

            if (component.WaterVertices == null)
                component.WaterVertices = new List<TerrainVertex>();
            else
                component.WaterVertices.Clear();

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
                        var state = component.VoxelData[x, y, z];
                        if (state == 0)
                            continue;

                        var blockIndex = state;
                        var block = BlockRegistry.GetBlock(blockIndex);
                        if (block.Mesh == null) continue;

                        CullFaceDirection toCull = CullFaceDirection.None;

                        if (ShouldRenderFace(component, x, y, z + 1, blockIndex) == false)
                            toCull |= CullFaceDirection.North;

                        if (ShouldRenderFace(component, x, y, z - 1, blockIndex) == false)
                            toCull |= CullFaceDirection.South;

                        if (ShouldRenderFace(component, x, y + 1, z, blockIndex) == false)
                            toCull |= CullFaceDirection.Up;

                        if (ShouldRenderFace(component, x, y - 1, z, blockIndex) == false)
                            toCull |= CullFaceDirection.Down;

                        if (ShouldRenderFace(component, x + 1, y, z, blockIndex) == false)
                            toCull |= CullFaceDirection.West;

                        if (ShouldRenderFace(component, x - 1, y, z, blockIndex) == false)
                            toCull |= CullFaceDirection.East;

                        foreach (var face in block.Mesh.Faces)
                        {
                            if ((toCull & face.Key) != face.Key)
                            {
                                for (var i = 0; i < face.Value.Positions.Count; i++)
                                {
                                    var pos = face.Value.Positions[i] + new Vector3D<float>(x, y, z);

                                    var uv = face.Value.UVs[i];
                                    var col = face.Value.TintIndicies[i] == null
                                        ? new Vector4D<float>(255, 255, 255, 0.0f)
                                        : new Vector4D<float>(10, 204, 66, 0.0f);

                                    var vert = new TerrainVertex()
                                    {
                                        position = (Vector3D<byte>)pos,
                                        colour = (Vector4D<byte>)col,
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
            TerrainChunkBatcherRenderer.QueueChunkForBatching(component);
        }

        bool ShouldRenderFace(TerrainChunkComponent component, int x, int y, int z, int workingBlockId)
        {
            var blockId = component.GetBlockId(x, y, z);
            if (blockId == 0)
                return true;

            var block = BlockRegistry.GetBlock(blockId);
            if (block.IsTransparent)
            {
                if (blockId != workingBlockId)
                    return true;

                if (block.TransparentCullSelf)
                    return false;

                return true;
            }

            return false;
        }

    }
}
