using AbsEngine;
using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsEngine.ECS.Systems;
using AbsEngine.Rendering;
using AbsGameProject.Terrain;
using Silk.NET.Maths;
using System.Drawing;

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
            var scene = Scene.Load();

            var camEnt = scene.EntityManager.CreateEntity();
            var cam = camEnt.AddComponent<CameraComponent>();

            scene.RegisterSystem<FlyCamSystem>();
            scene.RegisterSystem<TerrainChunkGeneratorSystem>();
            scene.RegisterSystem<TerrainNoiseGeneratorSystem>();
            scene.RegisterSystem<TerrainMeshGeneratorSystem>();
            scene.RegisterSystem<MeshRendererSystem>();
        }
    }
}