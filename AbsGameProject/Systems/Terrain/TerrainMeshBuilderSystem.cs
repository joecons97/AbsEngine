using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsEngine.Rendering;
using AbsGameProject.Components.Terrain;
using AbsGameProject.Textures;
using Silk.NET.Maths;

namespace AbsGameProject.Systems.Terrain
{
    public class TerrainMeshBuilderSystem : ComponentSystem<TerrainChunkComponent>
    {
        private Material waterMaterial;
        private Material terrainMaterial;

        protected override Func<TerrainChunkComponent, bool>? Predicate =>
            (x) => x.State == TerrainChunkComponent.TerrainState.MeshConstructed;

        protected override int MaxIterationsPerFrame => 1;

        public TerrainMeshBuilderSystem(Scene scene) : base(scene)
        {
            terrainMaterial = new Material("TerrainShader");
            if (TextureAtlas.AtlasTexture != null)
                terrainMaterial.SetTexture("uAtlas", TextureAtlas.AtlasTexture);

            waterMaterial = new Material("WaterShader");
            if (TextureAtlas.AtlasTexture != null)
                waterMaterial.SetTexture("uAtlas", TextureAtlas.AtlasTexture);
        }

        public override void OnTick(TerrainChunkComponent component, float deltaTime)
        {
            if (component.Mesh == null || component.WaterMesh == null || component.Mesh.HasBeenBuilt)
                return;

            component.Mesh.Build();
            component.WaterMesh.Build();

            if (component.Renderer != null)
            {
                component.Renderer.Mesh = component.Mesh;
                component.Renderer.Material = terrainMaterial;
                component.Renderer.BoundingBox = component.BoundingBox?.Transform(component.Entity.Transform.LocalPosition, Vector3D<float>.One);   
            }

            if(component.WaterRenderer != null)
            {
                component.WaterRenderer.Mesh = component.WaterMesh;
                component.WaterRenderer.Material = waterMaterial;
                component.WaterRenderer.BoundingBox = component.BoundingBox?.Transform(component.Entity.Transform.LocalPosition, Vector3D<float>.One);
            }

            component.State = TerrainChunkComponent.TerrainState.Done;
        }
    }
}
