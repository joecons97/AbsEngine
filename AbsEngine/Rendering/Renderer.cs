using AbsEngine.ECS.Components;
using AbsEngine.Exceptions;
using AbsEngine.Maths;
using AbsEngine.Physics;
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
    private static readonly Queue<RenderJob> renderQueue = new Queue<RenderJob>();

    public static void Render(Mesh mesh, Material material, Matrix4X4<float> trs, BoundingBox? boundingBox = null)
    {
        renderQueue.Enqueue(new RenderJob(mesh, material, trs, boundingBox));
    }

    internal static void CompleteFrame()
    {
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

        while (renderQueue.Count > 0)
        {
            var r = renderQueue.Dequeue();

            if (r.BoundingBox != null && !frustum.Intersects(r.BoundingBox))
                continue;

            r.Material.Bind();
            r.Mesh.Bind();

            r.Material.SetMatrix("uWorldMatrix", r.WorldMatrix);
            r.Material.SetMatrix("uMvp", r.WorldMatrix * vpMat);

            if (r.Mesh.UseTriangles)
                game.Graphics.DrawElements((uint)r.Mesh.Triangles.Length);
            else
                game.Graphics.DrawArrays((uint)r.Mesh.Positions.Length);
        }
    }
}
