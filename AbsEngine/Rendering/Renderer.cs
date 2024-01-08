using AbsEngine.ECS.Components;
using AbsEngine.Exceptions;
using AbsEngine.IO;
using AbsEngine.Physics;
using AbsEngine.Rendering.RenderCommand;
using Assimp;
using ImGuiNET;
using Silk.NET.Maths;
using System.Diagnostics;
using System.Numerics;

namespace AbsEngine.Rendering;

public static class Renderer
{
    public const int TRANSPARENT_QUEUE_POSITION = 1000;

    internal static readonly Mesh BLIT_QUAD;
    private static readonly List<IRenderCommand> renderQueue = new List<IRenderCommand>();
    private static readonly List<FullscreenEffect> effects = new List<FullscreenEffect>();  

    private static readonly RenderTexture backBufferRenderTexture;
    private static readonly RenderTexture backBufferForShaders;
    private static readonly RenderTexture postProcessingRenderTexture;

    private static readonly Material backBufferMaterial;

    private static float _fps = 0;
    private static int _drawCalls = 0;
    private static int _culledDrawCalls = 0;

    private static float _fpsTime = 0;

    private static bool _displayDebug = false;

    static Renderer()
    {
        if (Game.Instance == null)
            throw new GameInstanceException();

        backBufferRenderTexture = new RenderTexture(Game.Instance.Window.Size);
        backBufferForShaders = new RenderTexture(Game.Instance.Window.Size);
        postProcessingRenderTexture = new RenderTexture(Game.Instance.Window.Size);

        backBufferMaterial = new Material("BackBuffer");
        BLIT_QUAD = MeshLoader.LoadMesh("Engine/Meshes/Quad.fbx");

        Game.Instance.Window.Resize += Window_Resize;
    }

    internal static void AddEffect<T>() where T : FullscreenEffect
    {
        var type = typeof(T);
        if (effects.Any(x => x.GetType() == type))
            throw new InvalidOperationException($"Cannot add the fullscreen effect of type {type} because it has already been registered.");

        var instance = (FullscreenEffect?)Activator.CreateInstance(type);
        if (instance == null)
            throw new Exception($"An error occurred whilst instantiating fullscreen effect of type {type}");

        effects.Add(instance);
    }

    internal static void RemoveEffect<T>() where T : FullscreenEffect
    {
        var type = typeof(T);
        var effect = effects.FirstOrDefault(x => x.GetType() == type);

        if (effect == null)
            throw new InvalidOperationException($"Cannot remove the fullscreen effect of type {type} because it has not been registered.");

        effects.Remove(effect);
    }

    internal static T? GetEffect<T>() where T : FullscreenEffect
    {
        var type = typeof(T);
        var effect = effects.FirstOrDefault(x => x.GetType() == type);
        if(effect == null) 
            return null;

        return (T)effect;
    }

    private static void Window_Resize(Vector2D<int> obj)
    {
        backBufferRenderTexture.SetSize(obj);
        backBufferForShaders.SetSize(obj);
        postProcessingRenderTexture.SetSize(obj);
    }

    public static void Render(Mesh mesh, Material material, Matrix4X4<float> trs, BoundingBox? boundingBox = null)
    {
        int renderPos = material.Shader.GetBackendShader().GetRenderQueuePosition();
        int pos = Math.Min(renderQueue.Count, renderPos);
        var drawCall = new SingleDrawRenderCommand(mesh, material, trs, boundingBox, renderPos);
        renderQueue.Insert(pos, drawCall);
    }

    public static void Render(IRenderCommand renderCommand)
    {
        int renderPos = renderCommand.RenderQueuePosition;
        int pos = Math.Min(renderQueue.Count, renderPos);
        renderQueue.Insert(pos, renderCommand);
    }

    static void ClearRenderTexture(Game game, RenderTexture renderTexture)
    {
        renderTexture.Bind();
        game.Graphics.ClearScreen(System.Drawing.Color.CornflowerBlue);
        renderTexture.UnBind();
    }

    static CameraComponent? GetActiveCamera(Game game)
    {
        return !SceneCameraComponent.IsInSceneView 
            ? game._activeScenes.FirstOrDefault()?.EntityManager.GetComponents<CameraComponent>(x => x.IsMainCamera).FirstOrDefault()
            : game._activeScenes.FirstOrDefault()?.EntityManager.GetComponents<SceneCameraComponent>().FirstOrDefault();
    }

    internal static void CompleteFrame()
    {
        _culledDrawCalls = 0;
        _drawCalls = 0;

        var game = Game.Instance;
        if (game == null)
            throw new GameInstanceException();

        using (Profiler.BeginEvent("Clear RenderTargets"))
        {
            if (RenderTexture.Active != null)
            {
                ClearRenderTexture(game, RenderTexture.Active);
            }
            else
            {
                ClearRenderTexture(game, backBufferForShaders);

                ClearRenderTexture(game, backBufferRenderTexture);
            }
        }

        RenderTexture renderTarget;
        if (RenderTexture.Active != null)
        {
            renderTarget = RenderTexture.Active;
        }
        else
        {
            renderTarget = backBufferRenderTexture;
        }

        renderTarget.Bind();

        var cam = GetActiveCamera(game);

        if (cam == null)
            throw new Exception("Cannot complete frame, Main camera is null");

        var frustum = cam.GetFrustum();

        var trans = cam.Entity.Transform;

        BindDefaultUniforms(cam, renderTarget);

        bool hasBlitToShaderBuffer = false;

        using (Profiler.BeginEvent("Render Queue"))
        {
            while (renderQueue.Count > 0)
            {
                _drawCalls++;
                var r = renderQueue.First();
                renderQueue.RemoveAt(0);

                if (r.RenderQueuePosition >= TRANSPARENT_QUEUE_POSITION && hasBlitToShaderBuffer == false)
                {
                    using (Profiler.BeginEvent("Blit to transparency texture"))
                    {
                        backBufferRenderTexture.BlitTo(backBufferForShaders);
                        hasBlitToShaderBuffer = true;
                    }
                }

                if (r.ShouldCull(frustum))
                {
                    _culledDrawCalls++;
                    continue;
                }

                r.Render(game.Graphics, cam, backBufferForShaders);
            }
        }

        renderTarget.UnBind();

        using (Profiler.BeginEvent("Render Post Processing Effects"))
        {
            var currentRt = backBufferRenderTexture;

            foreach (var item in effects)
            {
                ClearRenderTexture(game, postProcessingRenderTexture);

                item.OnRender(currentRt, postProcessingRenderTexture);

                currentRt = postProcessingRenderTexture;
            }

            if (effects.Count > 0)
                postProcessingRenderTexture.BlitTo(backBufferRenderTexture);
        }

        FinaliseRender(game);

#if DEBUG
        DrawDebug();
#endif

        Shader._dirtyGlobalVariables.Clear();
    }

    static internal void BindDefaultUniforms(CameraComponent cam, RenderTexture renderTarget)
    {
        using (Profiler.BeginEvent("BindDefaultUniforms"))
        {
            Shader.SetGlobalVector("_Resolution", (Vector2D<float>)renderTarget!.ColorTexture.Size);
            Shader.SetGlobalFloat("_NearClipPlane", cam.NearClipPlane);
            Shader.SetGlobalFloat("_FarClipPlane", cam.FarClipPlane);
            Shader.SetGlobalVector("_CameraPosition", cam.Entity.Transform.Position);
            Shader.SetGlobalFloat("_Time", Game.Instance!.Time);
            Shader.SetGlobalFloat("_DeltaTime", Game.Instance!.DeltaTime);
        }
    }

    static void FinaliseRender(Game game)
    {
        using (Profiler.BeginEvent("FinaliseRender"))
        {
            game.Graphics.ClearScreen(System.Drawing.Color.CornflowerBlue);

            backBufferMaterial.SetTexture("_ColorMap", backBufferRenderTexture.ColorTexture);
            backBufferMaterial.Bind();
            BLIT_QUAD.Bind();

            game.Graphics.DrawElements((uint)BLIT_QUAD.Triangles.Length);
        }
    }

    static void DrawDebug()
    {
        if (SceneCameraComponent.IsInSceneView)
        {
            if (_fpsTime > 1f)
            {
                _fps = 1.0f / Game.Instance!.DeltaTime;
                _fpsTime = 0;
            }

            _fpsTime += Game.Instance!.DeltaTime;

            ImGui.SetNextWindowPos(new Vector2(0, 0));
            var size = ImGui.GetIO().DisplaySize;
            ImGui.SetNextWindowSize(size);

            ImGui.Begin("", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.MenuBar);

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.Selectable("Display Time Info", _displayDebug))
                {
                    _displayDebug = !_displayDebug;
                }
            }
            ImGui.End();
        }

        if (_displayDebug)
        {
            ImGui.SetNextWindowPos(new Vector2(0, 24));
            ImGui.SetNextWindowBgAlpha(0.5f);

            ImGui.Begin("Renderer", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize);

            ImGui.Value("FPS", _fps);
            ImGui.Value("Time", Game.Instance!.Time);
            ImGui.Value("Total Draw Calls", _drawCalls);
            ImGui.Value("Draw Calls", _drawCalls - _culledDrawCalls);
            ImGui.Value("Culled Draw Calls", _culledDrawCalls);

            ImGui.End();
        }
    }
}
