using AbsEngine;
using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsEngine.ECS.Systems;
using AbsEngine.Rendering;
using AbsGameProject.Blocks;
using AbsGameProject.Terrain;
using AbsGameProject.Textures;
using Silk.NET.Maths;

namespace AbsGameProject
{
    public class GameProject
    {
        public static void Main()
        {
            _ = new Game("Josephus", "Test Game", GraphicsAPIs.OpenGL, new Vector2D<int>(800, 600));
            if (Game.Instance == null)
                return;

            Game.Instance.OnLoad += Instance_OnLoad;

            Game.Instance.Run();
        }

        private static void Instance_OnLoad()
        {
            TextureAtlas.Initialise(1024, 0);

            BlockRegistry.AddBlock(Block.New("stone", "Stone").WithVoxelModel("Content/Models/Blocks/Cube.json").Build());
            BlockRegistry.AddBlock(Block.New("dirt", "Dirt").WithVoxelModel("Content/Models/Blocks/Dirt.json").Build());
            BlockRegistry.AddBlock(Block.New("grass", "Grass").WithVoxelModel("Content/Models/Blocks/Grass.json").Build());

            TextureAtlas.Build();

            var scene = Scene.Load();

            var camEnt = scene.EntityManager.CreateEntity();
            var cam = camEnt.AddComponent<CameraComponent>();

            scene.RegisterSystem<FlyCamSystem>();
            scene.RegisterSystem<TerrainChunkGeneratorSystem>();
            scene.RegisterSystem<TerrainNoiseGeneratorSystem>();
            scene.RegisterSystem<TerrainMeshConstructorSystem>();
            scene.RegisterSystem<TerrainMeshBuilderSystem>();
            scene.RegisterSystem<MeshRendererSystem>();

            var cubeEnt = scene.EntityManager.CreateEntity();
            var renderer = cubeEnt.AddComponent<MeshRendererComponent>();
        }
    }
}