using AbsEngine.ECS.Components;
using AbsEngine.Exceptions;
using AbsEngine.Maths;
using AbsEngine.Physics;
using ImGuiNET;
using Silk.NET.Maths;

namespace AbsEngine.Rendering;

public class RenderJob
{
    public Mesh Mesh { get; set; }
    public Material Material { get; set; }
    public Matrix4X4<float> WorldMatrix { get; set; }
    public BoundingBox? BoundingBox { get; set; }

    public RenderJob(Mesh mesh, Material material, Matrix4X4<float> worldMatrix, BoundingBox? boundingBox)
    {
        Mesh = mesh;
        Material = material;
        WorldMatrix = worldMatrix;
        BoundingBox = boundingBox;
    }
}

public static class Renderer
{
    private static readonly List<RenderJob> renderQueue = new List<RenderJob>();

    private static float _fps = 0;
    private static int _drawCalls = 0;
    private static int _culledDrawCalls = 0;

    private static float _fpsTime = 0;

    public static void Render(Mesh mesh, Material material, Matrix4X4<float> trs, BoundingBox? boundingBox = null)
    {
        int pos = Math.Min(renderQueue.Count, material.Shader.GetBackendShader().GetRenderQueuePosition());
        renderQueue.Insert(pos, new RenderJob(mesh, material, trs, boundingBox));
    }

    internal static void CompleteFrame()
    {
        _culledDrawCalls = 0;
        _drawCalls = 0;

        var game = Game.Instance;
        if (game == null)
            throw new GameInstanceException();

        var cam = !SceneCameraComponent.IsInSceneView ?
            game._activeScenes.FirstOrDefault()?.EntityManager.GetComponents<CameraComponent>(x => x.IsMainCamera).FirstOrDefault()
            : game._activeScenes.FirstOrDefault()?.EntityManager.GetComponents<SceneCameraComponent>().FirstOrDefault();

        if (cam == null)
            throw new Exception("Cannot complete frame, Main camera is null");

        var trans = cam.Entity.Transform;

        Matrix4X4.Invert(trans.WorldMatrix, out var viewMat);
        var winX = (float)game.Window.FramebufferSize.X;
        var winY = (float)game.Window.FramebufferSize.Y;
        var projMat = Matrix4X4.CreatePerspectiveFieldOfView(cam.FieldOfView * AbsMaths.DEG_2_RAD,
        winX / winY, cam.NearClipPlane, cam.FarClipPlane);

        var vpMat = viewMat * projMat;
        var frustum = new Frustum(vpMat);

        Shader.SetGlobalVector("_CameraPosition", trans.Position);
        Shader.SetGlobalFloat("_Time", Game.Instance!.Time);
        Shader.SetGlobalFloat("_DeltaTime", Game.Instance!.DeltaTime);

        while (renderQueue.Count > 0)
        {
            _drawCalls++;
            var r = renderQueue.First();
            renderQueue.RemoveAt(0);

            if (r.BoundingBox != null && !frustum.Intersects(r.BoundingBox))
            {
                _culledDrawCalls++;
                continue;
            }

            r.Material.Bind();
            r.Mesh.Bind();

            r.Material.SetMatrix("uWorldMatrix", r.WorldMatrix);
            r.Material.SetMatrix("uMvp", r.WorldMatrix * vpMat);

            if (r.Mesh.UseTriangles && r.Mesh.Triangles.Length > 0)
                game.Graphics.DrawElements((uint)r.Mesh.Triangles.Length);
            else if(r.Mesh.VertexCount > 0)
                game.Graphics.DrawArrays((uint)r.Mesh.VertexCount);
        }

        if (SceneCameraComponent.IsInSceneView)
        {
            if (_fpsTime > 0.5f)
            {
                _fps = 1.0f / Game.Instance!.DeltaTime;
                _fpsTime = 0;
            }

            _fpsTime += Game.Instance!.DeltaTime;

            ImGui.Begin("Renderer");

            ImGui.Value("FPS", _fps);
            ImGui.Value("Time", Game.Instance!.Time);
            ImGui.Value("Total Draw Calls", _drawCalls);
            ImGui.Value("Draw Calls", _drawCalls - _culledDrawCalls);
            ImGui.Value("Culled Draw Calls", _culledDrawCalls);

            ImGui.End();
        }
    }
}
