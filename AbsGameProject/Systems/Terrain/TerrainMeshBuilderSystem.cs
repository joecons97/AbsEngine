using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsEngine.Rendering;
using AbsGameProject.Components.Terrain;
using AbsGameProject.Textures;

namespace AbsGameProject.Systems.Terrain
{
    public class TerrainMeshBuilderSystem : ComponentSystem<TerrainChunkComponent>
    {
        private Material material;

        protected override Func<TerrainChunkComponent, bool>? Predicate =>
            (x) => x.State == TerrainChunkComponent.TerrainState.MeshConstructed;

        protected override int MaxIterationsPerFrame => 1;

        public TerrainMeshBuilderSystem(Scene scene) : base(scene)
        {
            material = new Material("TerrainShader");
            if (TextureAtlas.AtlasTexture != null)
                material.SetTexture("uAtlas", TextureAtlas.AtlasTexture);
        }

        public override void OnTick(TerrainChunkComponent component, float deltaTime)
        {
            if (component.Mesh == null || component.Mesh.HasBeenBuilt)
                return;

            var renderer = component.Entity.GetComponent<MeshRendererComponent>();

            component.Mesh.Build();

            if (renderer != null)
            {
                renderer.Mesh = component.Mesh;
                renderer.Material = material;
            }

            component.State = TerrainChunkComponent.TerrainState.Done;
        }
    }
}
