using AbsEngine.ECS.Components;
using AbsEngine.Exceptions;
using AbsEngine.Maths;
using Silk.NET.Maths;

namespace AbsEngine.Rendering;

public static class Renderer
{
    private static readonly Queue<(Mesh, Material, Matrix4X4<float>)> renderQueue = new Queue<(Mesh, Material, Matrix4X4<float>)>();

    public static void Render(Mesh mesh, Material material, Matrix4X4<float> trs)
    {
        renderQueue.Enqueue((mesh, material, trs));
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

        while (renderQueue.Count > 0)
        {
            var r = renderQueue.Dequeue();

            r.Item2.Bind();
            r.Item1.Bind();

            r.Item2.SetMatrix("uWorldMatrix", r.Item3);
            r.Item2.SetMatrix("uMvp", r.Item3 * vpMat);

            if(r.Item1.UseTriangles)
                game.Graphics.DrawElements((uint)r.Item1.Triangles.Length);
            else
                game.Graphics.DrawArrays((uint)r.Item1.Positions.Length);
        }
    }
}
