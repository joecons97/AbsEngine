using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsEngine.Rendering;
using Silk.NET.Maths;

namespace AbsGameProject.Terrain
{
    public class TerrainMeshGeneratorSystem : ComponentSystem<TerrainChunkComponent>
    {
        private Material material;

        protected override Func<TerrainChunkComponent, bool>? Predicate =>
            (x) => x.State == TerrainChunkComponent.TerrainState.NoiseGenerated && x.HasAllNeighbours;

        protected override int MaxIterationsPerFrame => 1;

        public TerrainMeshGeneratorSystem(Scene scene) : base(scene)
        {
            material = new Material("TerrainShader");
        }

        public override void OnTick(TerrainChunkComponent component, float deltaTime)
        {
            if (component.VoxelData == null)
                return;

            var renderer = component.Entity.GetComponent<MeshRendererComponent>();

            var mesh = new Mesh();

            Task.Run(() =>
            {
                uint indexCount = 0;

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

                            //Front Face
                            if (ShouldRenderFace(x, y, z + 1))
                            {
                                vertices.Add(new Vector3D<float>(1 + x, 1 + y, 1 + z));
                                vertices.Add(new Vector3D<float>(0 + x, 1 + y, 1 + z));
                                vertices.Add(new Vector3D<float>(0 + x, 0 + y, 1 + z));
                                vertices.Add(new Vector3D<float>(1 + x, 0 + y, 1 + z));

                                normals.Add(new Vector3D<float>(0, 0, 1));
                                normals.Add(new Vector3D<float>(0, 0, 1));
                                normals.Add(new Vector3D<float>(0, 0, 1));
                                normals.Add(new Vector3D<float>(0, 0, 1));

                                indices.Add(indexCount);
                                indices.Add(indexCount + 1);
                                indices.Add(indexCount + 2);

                                indices.Add(indexCount + 2);
                                indices.Add(indexCount + 3);
                                indices.Add(indexCount);

                                indexCount += 4;
                            }

                            //Back
                            if (ShouldRenderFace(x, y, z - 1))
                            {
                                vertices.Add(new Vector3D<float>(0 + x, 1 + y, 0 + z));
                                vertices.Add(new Vector3D<float>(1 + x, 1 + y, 0 + z));
                                vertices.Add(new Vector3D<float>(1 + x, 0 + y, 0 + z));
                                vertices.Add(new Vector3D<float>(0 + x, 0 + y, 0 + z));

                                normals.Add(new Vector3D<float>(0, 0, -1));
                                normals.Add(new Vector3D<float>(0, 0, -1));
                                normals.Add(new Vector3D<float>(0, 0, -1));
                                normals.Add(new Vector3D<float>(0, 0, -1));

                                indices.Add(indexCount);
                                indices.Add(indexCount + 1);
                                indices.Add(indexCount + 2);

                                indices.Add(indexCount + 2);
                                indices.Add(indexCount + 3);
                                indices.Add(indexCount);

                                indexCount += 4;
                            }

                            //Top
                            if (ShouldRenderFace(x, y + 1, z))
                            {
                                vertices.Add(new Vector3D<float>(1 + x, 1 + y, 0 + z));
                                vertices.Add(new Vector3D<float>(0 + x, 1 + y, 0 + z));
                                vertices.Add(new Vector3D<float>(0 + x, 1 + y, 1 + z));
                                vertices.Add(new Vector3D<float>(1 + x, 1 + y, 1 + z));

                                normals.Add(new Vector3D<float>(0, 1, 0));
                                normals.Add(new Vector3D<float>(0, 1, 0));
                                normals.Add(new Vector3D<float>(0, 1, 0));
                                normals.Add(new Vector3D<float>(0, 1, 0));

                                indices.Add(indexCount);
                                indices.Add(indexCount + 1);
                                indices.Add(indexCount + 2);

                                indices.Add(indexCount + 2);
                                indices.Add(indexCount + 3);
                                indices.Add(indexCount);

                                indexCount += 4;
                            }

                            //Bottom
                            if (ShouldRenderFace(x, y - 1, z))
                            {
                                vertices.Add(new Vector3D<float>(1 + x, 0 + y, 1 + z));
                                vertices.Add(new Vector3D<float>(0 + x, 0 + y, 1 + z));
                                vertices.Add(new Vector3D<float>(0 + x, 0 + y, 0 + z));
                                vertices.Add(new Vector3D<float>(1 + x, 0 + y, 0 + z));

                                normals.Add(new Vector3D<float>(0, -1, 0));
                                normals.Add(new Vector3D<float>(0, -1, 0));
                                normals.Add(new Vector3D<float>(0, -1, 0));
                                normals.Add(new Vector3D<float>(0, -1, 0));

                                indices.Add(indexCount);
                                indices.Add(indexCount + 1);
                                indices.Add(indexCount + 2);

                                indices.Add(indexCount + 2);
                                indices.Add(indexCount + 3);
                                indices.Add(indexCount);

                                indexCount += 4;
                            }

                            //Right
                            if (ShouldRenderFace(x + 1, y, z))
                            {
                                vertices.Add(new Vector3D<float>(1 + x, 1 + y, 0 + z));
                                vertices.Add(new Vector3D<float>(1 + x, 1 + y, 1 + z));
                                vertices.Add(new Vector3D<float>(1 + x, 0 + y, 1 + z));
                                vertices.Add(new Vector3D<float>(1 + x, 0 + y, 0 + z));

                                normals.Add(new Vector3D<float>(1, 0, 0));
                                normals.Add(new Vector3D<float>(1, 0, 0));
                                normals.Add(new Vector3D<float>(1, 0, 0));
                                normals.Add(new Vector3D<float>(1, 0, 0));

                                indices.Add(indexCount);
                                indices.Add(indexCount + 1);
                                indices.Add(indexCount + 2);

                                indices.Add(indexCount + 2);
                                indices.Add(indexCount + 3);
                                indices.Add(indexCount);

                                indexCount += 4;
                            }

                            //Left
                            if (ShouldRenderFace(x - 1, y, z))
                            {
                                vertices.Add(new Vector3D<float>(0 + x, 1 + y, 1 + z));
                                vertices.Add(new Vector3D<float>(0 + x, 1 + y, 0 + z));
                                vertices.Add(new Vector3D<float>(0 + x, 0 + y, 0 + z));
                                vertices.Add(new Vector3D<float>(0 + x, 0 + y, 1 + z));

                                normals.Add(new Vector3D<float>(-1, 0, 0));
                                normals.Add(new Vector3D<float>(-1, 0, 0));
                                normals.Add(new Vector3D<float>(-1, 0, 0));
                                normals.Add(new Vector3D<float>(-1, 0, 0));

                                indices.Add(indexCount);
                                indices.Add(indexCount + 1);
                                indices.Add(indexCount + 2);

                                indices.Add(indexCount + 2);
                                indices.Add(indexCount + 3);
                                indices.Add(indexCount);

                                indexCount += 4;
                            }
                        }
                    }
                }

                mesh.Triangles = indices.ToArray();
                mesh.Positions = vertices.ToArray();
                mesh.Normals = normals.ToArray();
                mesh.Tangents = new Vector3D<float>[0];
                mesh.Colours = new Vector4D<float>[0];
                mesh.Uvs = new Vector2D<float>[0];
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
                    return true;

                if (component.VoxelData == null)
                    return false;

                return component.VoxelData[x, y, z] == 0;
            }

        }
    }
}
