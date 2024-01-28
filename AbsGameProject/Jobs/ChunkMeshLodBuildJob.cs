using AbsGameProject.Blocks;
using AbsGameProject.Components.Terrain;
using AbsGameProject.Maths;
using AbsGameProject.Models.Meshing;
using AbsGameProject.Structures;
using Schedulers;
using Silk.NET.Maths;

namespace AbsGameProject.Jobs
{
    public class ChunkMeshBuildLodJob : IJob
    {
        public TerrainChunkComponent component;

        public ChunkMeshBuildLodJob(TerrainChunkComponent component)
        {
            this.component = component;
        }

        public void Execute()
        {
            if (component.State == TerrainChunkComponent.TerrainState.MeshConstructing)
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

            for (int x = 0; x < TerrainChunkComponent.WIDTH; x += component.Scale)
            {
                for (int z = 0; z < TerrainChunkComponent.WIDTH; z += component.Scale)
                {
                    int worldX = (int)(x + component.Entity.Transform.LocalPosition.X);
                    int worldZ = (int)(z + component.Entity.Transform.LocalPosition.Z);
                    var y = (int)Heightmap.GetHeightAt(worldX, worldZ);

                    var blockIndex = 3;

                    if (y < TerrainChunkComponent.WATER_HEIGHT - 1)
                    {
                        y = TerrainChunkComponent.WATER_HEIGHT - 1;
                        blockIndex = 5;
                    }

                    var block = BlockRegistry.GetBlock(blockIndex);
                    if (block.Mesh == null) continue;

                    CullFaceDirection toCull = CullFaceDirection.Down;

                    //if (ShouldRenderFace(worldX + component.Scale, y, worldZ) == false)
                    //    toCull |= CullFaceDirection.East;

                    //if (ShouldRenderFace(worldX - component.Scale, y, worldZ) == false)
                    //    toCull |= CullFaceDirection.East;

                    //if (ShouldRenderFace(worldX, y, worldZ + 1) == false)
                    //    toCull |= CullFaceDirection.North;

                    //if (ShouldRenderFace(worldX, y, worldZ - 1) == false)
                    //    toCull |= CullFaceDirection.South;

                    foreach (var face in block.Mesh.Faces)
                    {
                        if ((toCull & face.Key) != face.Key)
                        {
                            for (var i = 0; i < face.Value.Positions.Count; i++)
                            {
                                var pos = (face.Value.Positions[i] * component.Scale) + new Vector3D<float>(x, y, z);

                                var uv = face.Value.UVs[i];
                                var col = new Vector4D<float>(255, 255, 255, 0.0f);
                                if (face.Value.TintIndicies[i] != null)
                                    col = new Vector4D<float>(10, 204, 66, 0.0f);
                                else if (block.Id == "water")
                                    col = new Vector4D<float>(24, 154, 227, 0.0f);

                                var vert = new TerrainVertex()
                                {
                                    position = (Vector3D<byte>)pos,
                                    colour = (Vector4D<byte>)col,
                                    uv = (Vector2D<Half>)uv
                                };

                                component.TerrainVertices.Add(vert);

                            }
                        }
                    }
                }
            }

            component.State = TerrainChunkComponent.TerrainState.MeshConstructed;
            component.IsMeshBeingConstructed = false;
        }

        bool ShouldRenderFace(int x, int y, int z)
        {
            var h = (int)Heightmap.GetHeightAt(x, z);
            if (y < TerrainChunkComponent.WATER_HEIGHT - 1)
                y = TerrainChunkComponent.WATER_HEIGHT - 1;

            if (h == y)
                return false;

            return true;

            //var blockId = component.GetBlockId(x, y, z);
            //if (blockId == 0)
            //    return true;

            //var block = BlockRegistry.GetBlock(blockId);
            //if (block.IsTransparent)
            //{
            //    if (blockId != workingBlockId)
            //        return true;

            //    if (block.TransparentCullSelf)
            //        return false;

            //    return true;
            //}

            //return false;
        }

    }
}
