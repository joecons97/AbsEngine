using AbsEngine.ECS;
using AbsEngine.Rendering;
using AbsGameProject.Blocks;
using AbsGameProject.Components.Terrain;
using AbsGameProject.Models;
using AbsGameProject.Textures;
using Silk.NET.Maths;

namespace AbsGameProject.Systems.Terrain
{
    public class TerrainMeshConstructorSystem : AsyncComponentSystem<TerrainChunkComponent>
    {
        private readonly Material material;

        protected override Func<TerrainChunkComponent, bool>? Predicate =>
            (x) => x.State == TerrainChunkComponent.TerrainState.NoiseGenerated && x.HasAllNeighbours;

        protected override int MaxIterationsPerFrame => 1;

        public TerrainMeshConstructorSystem(Scene scene) : base(scene)
        {
            material = new Material("TerrainShader");
            if (TextureAtlas.AtlasTexture != null)
                material.SetTexture("uAtlas", TextureAtlas.AtlasTexture);
        }

        public override async Task OnTickAsync(TerrainChunkComponent component, float deltaTime)
        {
            if (component.VoxelData == null)
                return;

            component.Mesh = null;

            await Task.Run(() =>
            {
                var mesh = new Mesh();
                var transparentMesh = new Mesh();

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
                                if (block.Id == "water")
                                {
                                    transparentVertices.AddRange(face.Value.Positions.Select(v => v + new Vector3D<float>(x, y, z)));

                                    transparentUvs.AddRange(face.Value.UVs);

                                    transparentColours.AddRange(face.Value.TintIndicies
                                        .Select(x => x == null
                                            ? Vector4D<float>.One
                                            : Vector4D<float>.UnitY));
                                }
                                else
                                {
                                    vertices.AddRange(face.Value.Positions.Select(v => v + new Vector3D<float>(x, y, z)));

                                    uvs.AddRange(face.Value.UVs);

                                    colours.AddRange(face.Value.TintIndicies
                                        .Select(x => x == null
                                            ? Vector4D<float>.One
                                            : Vector4D<float>.UnitY));
                                }
                            }
                        }
                    }
                }

                mesh.Positions = vertices.ToArray();
                mesh.Normals = Array.Empty<Vector3D<float>>();
                mesh.Tangents = Array.Empty<Vector3D<float>>();
                mesh.Colours = colours.ToArray();
                mesh.Uvs = uvs.ToArray();
                mesh.UseTriangles = false;

                transparentMesh.Positions = transparentVertices.ToArray();
                transparentMesh.Normals = Array.Empty<Vector3D<float>>();
                transparentMesh.Tangents = Array.Empty<Vector3D<float>>();
                transparentMesh.Colours = transparentColours.ToArray();
                transparentMesh.Uvs = transparentUvs.ToArray();
                transparentMesh.UseTriangles = false;

                component.Mesh = mesh;
                component.WaterMesh = transparentMesh;
            });

            component.State = TerrainChunkComponent.TerrainState.MeshConstructed;

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
