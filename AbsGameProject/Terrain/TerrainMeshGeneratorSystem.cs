using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsEngine.Rendering;
using AbsGameProject.Models;
using Silk.NET.Maths;
using static System.Reflection.Metadata.BlobBuilder;

namespace AbsGameProject.Terrain
{
    public class TerrainMeshGeneratorSystem : ComponentSystem<TerrainChunkComponent>
    {
        private Material material;
        private CullableMesh defaultModel;

        protected override Func<TerrainChunkComponent, bool>? Predicate =>
            (x) => x.State == TerrainChunkComponent.TerrainState.NoiseGenerated && x.HasAllNeighbours;

        protected override int MaxIterationsPerFrame => 1;

        public TerrainMeshGeneratorSystem(Scene scene) : base(scene)
        {
            material = new Material("TerrainShader");
            if (!VoxelModel.TryFromFile("Content/Models/Blocks/Cube.json", out var model))
            {
                throw new Exception("Unable to load Content/Models/Blocks/Cube.json");
            }

            CullableMesh.TryFromVoxelMesh(model!, out defaultModel!);
        }

        public override void OnTick(TerrainChunkComponent component, float deltaTime)
        {
            if (component.VoxelData == null)
                return;

            var renderer = component.Entity.GetComponent<MeshRendererComponent>();

            var mesh = new Mesh();

            Task.Run(() =>
            {
                var vertices = new List<Vector3D<float>>();
                var normals = new List<Vector3D<float>>();
                var indices = new List<uint>();
                for (int x = 0; x < TerrainChunkComponent.WIDTH; x++)
                {
                    for (int z = 0; z < TerrainChunkComponent.WIDTH; z++)
                    {
                        for (int y = 0; y < TerrainChunkComponent.HEIGHT; y++)
                        {
                            var state = component.VoxelData[x, y, z];
                            if (state == 0)
                                continue;

                            List<Vector3D<float>> verts = new();
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

                            verts.AddRange(defaultModel.verts.Where(x => !toCull.HasFlag(x.Key)).SelectMany(x => x.Value));
                            vertices.AddRange(verts.Select(v => v + new Vector3D<float>(x, y, z)));
                        }
                    }
                }

                mesh.Triangles = indices.ToArray();
                mesh.Positions = vertices.ToArray();
                mesh.Normals = normals.ToArray();
                mesh.Tangents = new Vector3D<float>[0];
                mesh.Colours = new Vector4D<float>[0];
                mesh.Uvs = new Vector2D<float>[0];
                mesh.UseTriangles = false;
            }).Wait();

            mesh.Build();

            if (renderer != null)
            {
                renderer.Mesh = mesh;
                renderer.Material = material;
            }

            component.State = TerrainChunkComponent.TerrainState.MeshGenerated;

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
