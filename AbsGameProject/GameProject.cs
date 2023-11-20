using AbsEngine;
using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsEngine.ECS.Components.Physics;
using AbsEngine.ECS.Extensions;
using AbsEngine.ECS.Systems;
using AbsEngine.Rendering;
using AbsGameProject.Blocks;
using AbsGameProject.Physics;
using AbsGameProject.Player;
using AbsGameProject.Systems;
using AbsGameProject.Terrain;
using AbsGameProject.Textures;
using Silk.NET.Maths;

namespace AbsGameProject
{
    public class GameProject
    {
        public static void Main()
        {
            var game = new Game("Josephus", "Test Game", GraphicsAPIs.OpenGL, new Vector2D<int>(800, 600));

            game.OnLoad += Instance_OnLoad;
            game.Run();
        }

        private static void Instance_OnLoad()
        {
            TextureAtlas.Initialise(1024, 0);

            BlockRegistry.AddBlock(Block.New("stone", "Stone").WithVoxelModel("Content/Models/Blocks/Cube.json").Build());
            BlockRegistry.AddBlock(Block.New("dirt", "Dirt").WithVoxelModel("Content/Models/Blocks/Dirt.json").Build());
            BlockRegistry.AddBlock(Block.New("grass", "Grass").WithVoxelModel("Content/Models/Blocks/Grass.json").Build());

            TextureAtlas.Build();

            var scene = Scene.Load();

            SetupPlayer(scene);

            scene.RegisterMeshRenderer();
            scene.RegisterSceneCamera();

            scene.RegisterSystem<TerrainChunkGeneratorSystem>();
            scene.RegisterSystem<TerrainNoiseGeneratorSystem>();
            scene.RegisterSystem<TerrainMeshConstructorSystem>();
            scene.RegisterSystem<TerrainMeshBuilderSystem>();
            scene.RegisterSystem<BlockBreakerSystem>();
            scene.RegisterSystem<FlyCamSystem>();
            //scene.RegisterSystem<VoxelRigidbodySimulationSystem>();
        }

        static void SetupPlayer(Scene scene)
        {
            var playerEntity = scene.EntityManager.CreateEntity("Player");
            playerEntity.Transform.LocalPosition = new Vector3D<float>(0, 50, 0);
            var collider = playerEntity.AddComponent<BoxColliderComponent>();
            playerEntity.AddComponent<VoxelRigidbodyComponent>();
            collider.Min = new Vector3D<float>(-0.5f, -2, -0.5f);
            collider.Max = new Vector3D<float>(0.5f, 0, 0.5f);

            var playerCamera = scene.EntityManager.CreateEntity("Player Camera");
            playerCamera.AddComponent<CameraComponent>();
            playerCamera.Transform.Parent = playerEntity.Transform;
            playerCamera.Transform.LocalPosition = Vector3D<float>.Zero;

            playerEntity.AddComponent<PlayerControllerComponent>();
        }
    }
}