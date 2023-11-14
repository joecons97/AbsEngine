using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsEngine.Rendering;
using AbsGameProject.Blocks;
using AbsGameProject.Models;
using AbsGameProject.Textures;
using Silk.NET.Maths;
using System.Linq;

namespace AbsGameProject.Terrain
{
    public class TerrainMeshConstructorSystem : AsyncComponentSystem<TerrainChunkComponent>
    {
        private Material material;

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

            component.State = TerrainChunkComponent.TerrainState.MeshConstructed;

            await Task.Run(() =>
            {
                var mesh = new Mesh();

                var vertices = new List<Vector3D<float>>();
                var colours = new List<Vector4D<float>>();
                var uvs = new List<Vector2D<float>>();

                for (int x = 0; x < TerrainChunkComponent.WIDTH; x++)
                {
                    for (int z = 0; z < TerrainChunkComponent.WIDTH; z++)
                    {
                        for (int y = 0; y < TerrainChunkComponent.HEIGHT; y++)
                        {
                            var state = component.VoxelData[x, y, z];
                            if (state == 0)
                                continue;

                            var blockIndex = state - 1;
                            var block = BlockRegistry.GetBlock(blockIndex);
                            if (block?.Mesh == null) continue;

                            CullFaceDirection toCull = CullFaceDirection.None;

                            if (ShouldRenderFace(x, y, z + 1) == false)
                                toCull |= CullFaceDirection.North;

                            if (ShouldRenderFace(x, y, z - 1) == false)
                                toCull |= CullFaceDirection.South;

                            if (ShouldRenderFace(x, y + 1, z) == false)
                                toCull |= CullFaceDirection.Down;

                            if (ShouldRenderFace(x, y - 1, z) == false)
                                toCull |= CullFaceDirection.Up;

                            if (ShouldRenderFace(x + 1, y, z) == false)
                                toCull |= CullFaceDirection.West;

                            if (ShouldRenderFace(x - 1, y, z) == false)
                                toCull |= CullFaceDirection.East;

                            var faces = block.Mesh.Faces
                                .Where(x => (toCull & x.Key) != x.Key);

                            vertices.AddRange(faces
                                .Select(a => a.Value.Positions.Select(v => v + new Vector3D<float>(x, y, z)))
                                .SelectMany(x => x));

                            uvs.AddRange(faces
                                .Select(a => a.Value.UVs)
                                .SelectMany(x => x));

                            colours.AddRange(faces
                                .Select(x => x.Value.TintIndicies
                                .Select(x => x == null
                                    ? Vector4D<float>.One
                                    : Vector4D<float>.UnitY))
                                .SelectMany(x => x));
                        }
                    }
                }

                mesh.Positions = vertices.ToArray();
                mesh.Normals = new Vector3D<float>[0];
                mesh.Tangents = new Vector3D<float>[0];
                mesh.Colours = colours.ToArray();
                mesh.Uvs = uvs.ToArray();
                mesh.UseTriangles = false;

                component.Mesh = mesh;
            });

            bool ShouldRenderFace(int x, int y, int z)
            {
                if (x <= -1)
                {
                    x = TerrainChunkComponent.WIDTH + x;

                    if (component.LeftNeighbour?.VoxelData != null)
                        return component.LeftNeighbour.VoxelData[x, y, z] == 0;

                    return true;
                }

                if (x >= TerrainChunkComponent.WIDTH)
                {
                    x = x - TerrainChunkComponent.WIDTH;

                    if (component.RightNeighbour?.VoxelData != null)
                        return component.RightNeighbour.VoxelData[x, y, z] == 0;

                    return true;
                }

                if (z <= -1)
                {
                    z = TerrainChunkComponent.WIDTH + z;

                    if (component.SouthNeighbour?.VoxelData != null)
                        return component.SouthNeighbour.VoxelData[x, y, z] == 0;

                    return true;
                }

                if (z >= TerrainChunkComponent.WIDTH)
                {
                    z = z - TerrainChunkComponent.WIDTH;

                    if (component.NorthNeighbour?.VoxelData != null)
                        return component.NorthNeighbour.VoxelData[x, y, z] == 0;

                    return true;
                }

                if (y <= 0 || y >= TerrainChunkComponent.HEIGHT - 1)
                    return false;

                if (component.VoxelData == null)
                    return false;

                return component.VoxelData[x, y, z] == 0;
            }
        }
    }
}
