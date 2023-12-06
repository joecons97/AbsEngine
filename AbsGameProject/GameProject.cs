using AbsEngine;
using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsEngine.ECS.Extensions;
using AbsEngine.Rendering;
using AbsGameProject.Blocks;
using AbsGameProject.Components.Player;
using AbsGameProject.Maths.Physics;
using AbsGameProject.Systems.Player;
using AbsGameProject.Systems.Terrain;
using AbsGameProject.Textures;
using ImGuiNET;
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

            BlockRegistry.AddBlock(Block.New("air", "Air").WithOpacity(0).Build());
            BlockRegistry.AddBlock(Block.New("stone", "Stone").WithVoxelModel("Content/Models/Blocks/Stone.json").Build());
            BlockRegistry.AddBlock(Block.New("dirt", "Dirt").WithVoxelModel("Content/Models/Blocks/Dirt.json").Build());
            BlockRegistry.AddBlock(Block.New("grass", "Grass").WithVoxelModel("Content/Models/Blocks/Grass.json").Build());
            BlockRegistry.AddBlock(Block.New("light", "Light").WithLight(15).WithVoxelModel("Content/Models/Blocks/Glowstone.json").Build());

            BlockRegistry.AddBlock(Block.New("water", "Water").WithTransparency(true).WithNoCollision().
                WithVoxelModel("Content/Models/Blocks/Water.json").Build());

            BlockRegistry.AddBlock(Block.New("sand", "Sand").WithVoxelModel("Content/Models/Blocks/Sand.json").Build());

            BlockRegistry.AddBlock(Block.New("log_oak", "Oak Log").
                WithVoxelModel("Content/Models/Blocks/Log.json").Build());
            BlockRegistry.AddBlock(Block.New("leaves_oak", "Oak Leaves").WithTransparency(true).
                WithVoxelModel("Content/Models/Blocks/Leaves.json").Build());

            TextureAtlas.Build();

            var scene = Scene.Load();

            SetupPlayer(scene);

            scene.RegisterMeshRenderer();
            scene.RegisterSceneCamera();

            scene.RegisterSystem<TerrainChunkGeneratorSystem>();
            scene.RegisterSystem<TerrainChunkRebuilderSystem>();
            scene.RegisterSystem<TerrainNoiseGeneratorSystem>();
            scene.RegisterSystem<TerrainDecoratorSystem>();
            scene.RegisterSystem<TerrainMeshConstructorSystem>();
            scene.RegisterSystem<TerrainMeshBuilderSystem>();
            scene.RegisterSystem<BlockBreakerSystem>();
            scene.RegisterSystem<VoxelRigidbodySimulationSystem>();
            scene.RegisterSystem<PlayerControllerSystem>();
        }

        static void SetupPlayer(Scene scene)
        {
            var playerEntity = scene.EntityManager.CreateEntity("Player");
            playerEntity.Transform.LocalPosition = new Vector3D<float>(0, 100, 0);
            var collider = playerEntity.AddComponent<VoxelBoxColliderComponent>();
            var rigidbody = playerEntity.AddComponent<VoxelRigidbodyComponent>();
            rigidbody.Mass = 1;
            collider.Min = new Vector3D<float>(-0.25f, 0, -0.25f);
            collider.Max = new Vector3D<float>(0.25f, 2, 0.25f);

            var playerCamera = scene.EntityManager.CreateEntity("Player Camera");
            playerCamera.AddComponent<CameraComponent>();
            playerCamera.Transform.Parent = playerEntity.Transform;
            playerCamera.Transform.LocalPosition = Vector3D<float>.Zero;

            playerEntity.AddComponent<PlayerControllerComponent>();
        }
    }
}