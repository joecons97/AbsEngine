using AbsEngine.ECS;
using AbsEngine.Rendering;
using AbsGameProject.Blocks;
using AbsGameProject.Components.Terrain;
using AbsGameProject.Models.Meshing;
using AbsGameProject.Textures;
using Silk.NET.Maths;
using System.Runtime.InteropServices;

namespace AbsGameProject.Systems.Terrain
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TerrainVertex
    {
        public Vector3D<byte> position;
        public Vector4D<byte> colour;
        public Vector2D<Half> uv;
    }

    public class TerrainMeshConstructorSystem : AsyncComponentSystem<TerrainChunkComponent>
    {
        private readonly Material material;

        protected override Func<TerrainChunkComponent, bool>? Predicate => (x) => x.IsReadyForMeshGeneration;

        protected override int MaxIterationsPerFrame => 1;

        private VertexAttributeDescriptor[] vertexLayout;

        public TerrainMeshConstructorSystem(Scene scene) : base(scene)
        {
            material = new Material("TerrainShader");
            if (TextureAtlas.AtlasTexture != null)
                material.SetTexture("uAtlas", TextureAtlas.AtlasTexture);

            vertexLayout = new VertexAttributeDescriptor[]
            {
                new VertexAttributeDescriptor(VertexAttributeFormat.SInt8, 3),
                new VertexAttributeDescriptor(VertexAttributeFormat.UNorm8, 4),
                new VertexAttributeDescriptor(VertexAttributeFormat.Float16, 2),
            };
        }

        public override async Task OnTickAsync(TerrainChunkComponent component, float deltaTime)
        {
            if (component.VoxelData == null)
                return;

            //component.Mesh = null;

            await Task.Run(() =>
            {
                if (component.TerrainVertices == null)
                    component.TerrainVertices = new List<TerrainVertex>();
                else
                    component.TerrainVertices.Clear();

                if (component.WaterVertices == null)
                    component.WaterVertices = new List<TerrainVertex>();
                else
                    component.WaterVertices.Clear();

                //var mesh = new Mesh();
                //mesh.SetVertexBufferLayout(vertexLayout);

                //var transparentMesh = new Mesh();
                //transparentMesh.SetVertexBufferLayout(vertexLayout);

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

                            if (ShouldRenderFace(x, y, z + 1, blockIndex) == false)
                                toCull |= CullFaceDirection.North;

                            if (ShouldRenderFace(x, y, z - 1, blockIndex) == false)
                                toCull |= CullFaceDirection.South;

                            if (ShouldRenderFace(x, y + 1, z, blockIndex) == false)
                                toCull |= CullFaceDirection.Up;

                            if (ShouldRenderFace(x, y - 1, z, blockIndex) == false)
                                toCull |= CullFaceDirection.Down;

                            if (ShouldRenderFace(x + 1, y, z, blockIndex) == false)
                                toCull |= CullFaceDirection.West;

                            if (ShouldRenderFace(x - 1, y, z, blockIndex) == false)
                                toCull |= CullFaceDirection.East;

                            var faces = block.Mesh.Faces
                                .Where(x => (toCull & x.Key) != x.Key);

                            foreach (var face in faces)
                            {
                                for (var i = 0; i < face.Value.Positions.Count; i++)
                                {
                                    var pos = face.Value.Positions[i] + new Vector3D<float>(x, y, z);// + component.Entity.Transform.LocalPosition;
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
            });

            component.State = TerrainChunkComponent.TerrainState.MeshConstructed;
            TerrainChunkBatcherRenderer.BatchQueue.Enqueue(component);

            bool ShouldRenderFace(int x, int y, int z, int workingBlockId)
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
}
