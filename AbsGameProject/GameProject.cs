using AbsEngine;
using AbsEngine.ECS;
using AbsEngine.ECS.Systems;
using AbsEngine.Rendering;
using Silk.NET.Maths;
using System.Drawing;

namespace AbsGameProject
{
    public class GameProject
    {
        static Scene scene;

        static MeshRendererSystem tempRenderer;

        public static void Main(string[] args)
        {
            new Game("Josephus", "Test Game", GraphicsAPIs.OpenGL, new Vector2D<int>(800, 600));
            if (Game.Instance == null)
                return;

            Game.Instance.OnLoad += Instance_OnLoad;
            Game.Instance.OnUpdate += Instance_OnUpdate;
            Game.Instance.OnRender += Instance_OnRender;

            Game.Instance.Run();
        }

        private static void Instance_OnLoad()
        {
            scene = Scene.Load("Content/Maps/RawScene.rsc");

            tempRenderer = new MeshRendererSystem(scene);
        }

        private static async Task Instance_OnUpdate(double arg)
        {
            await scene.Tick((float)arg);
        }

        private static Task Instance_OnRender(double arg)
        {
            Game.Instance!.Graphics.ClearScreen(Color.CornflowerBlue);

            tempRenderer.Tick((float)arg);

            return Task.CompletedTask;
        }
    }
}