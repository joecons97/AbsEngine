using AbsEngine;
using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsEngine.ECS.Extensions;
using AbsEngine.Rendering;
using AbsEngine.Rendering.FullscreenEffects;
using AbsGameProject.Blocks;
using AbsGameProject.Components.Physics;
using AbsGameProject.Components.Player;
using AbsGameProject.Systems.Physics;
using AbsGameProject.Systems.Player;
using AbsGameProject.Systems.Terrain;
using AbsGameProject.Textures;
using Silk.NET.Maths;

namespace AbsGameProject
{
    public class GameProject
    {
        static bool _isFullscreen;

        public static void Main()
        {
            var game = new Game("Josephus", "Test Game", GraphicsAPIs.OpenGL, new Vector2D<int>(800, 600));

            game.OnLoad += Instance_OnLoad;

            game.Run();
        }

        private static void GameProject_KeyDown(Silk.NET.Input.IKeyboard arg1, Silk.NET.Input.Key arg2, int arg3)
        {
            if (arg2 == Silk.NET.Input.Key.F11)
            {
                _isFullscreen = !_isFullscreen;
                Game.Instance?.Graphics.SetFullscreen(_isFullscreen);
            }
        }

        private static void Instance_OnLoad(Game game)
        {
            game.InputContext.Keyboards.First().KeyDown += GameProject_KeyDown;

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
            BlockRegistry.AddBlock(Block.New("leaves_oak", "Oak Leaves").WithTransparency(false).
                WithVoxelModel("Content/Models/Blocks/Leaves.json").Build());

            for (int i = 0; i < 9; i++)
            {
                TextureAtlas.InsertTextureFile($"Content\\Textures\\Blocks\\destroy_stage_{i}.png");
                var coord = TextureAtlas.BlockLocations[$"destroy_stage_{i}"];
                Shader.SetGlobalVector($"destroy_stage_{i}", coord.Origin.As<float>() / TextureAtlas.Size);
            }

            TextureAtlas.Build();

            var scene = Scene.Load();

            SetupPlayer(scene);

            scene.RegisterMeshRenderer();
            scene.RegisterSceneCamera();

            scene.RegisterSystem<TerrainChunkGeneratorSystem>();
            scene.RegisterSystem<TerrainNoiseGeneratorSystem>();
            scene.RegisterSystem<TerrainDecoratorSystem>();
            scene.RegisterSystem<TerrainMeshConstructorSystem>();
            scene.RegisterSystem<TerrainChunkBatcherRenderer>();
            scene.RegisterSystem<BlockBreakerSystem>();
            scene.RegisterSystem<VoxelRigidbodySimulationSystem>();
            scene.RegisterSystem<PlayerControllerSystem>();

            var toneMapper = game.AddEffect<Tonemapping>();
            //toneMapper.Mode = TonemapperMode.NaughtyDog;
            //game.AddEffect<Grayscale>();
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

            var playerCameraEnt = scene.EntityManager.CreateEntity("Player Camera");
            var camera = playerCameraEnt.AddComponent<CameraComponent>();
            camera.FieldOfView = 90;
            playerCameraEnt.Transform.Parent = playerEntity.Transform;
            playerCameraEnt.Transform.LocalPosition = Vector3D<float>.Zero;

            playerEntity.AddComponent<PlayerControllerComponent>();
        }
    }
}