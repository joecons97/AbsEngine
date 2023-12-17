using AbsEngine.ECS.Components;
using AbsEngine.Physics;
using Silk.NET.Maths;

namespace AbsEngine.Rendering.RenderCommand
{
    public class SingleDrawRenderCommand : IRenderCommand
    {
        public Mesh Mesh { get; set; }
        public Material Material { get; set; }
        public Matrix4X4<float> WorldMatrix { get; set; }
        public BoundingBox? BoundingBox { get; set; }
        public int RenderQueuePosition { get; set; }

        public SingleDrawRenderCommand(Mesh mesh, Material material, Matrix4X4<float> worldMatrix, BoundingBox? boundingBox, int renderQueuePosition)
        {
            Mesh = mesh;
            Material = material;
            WorldMatrix = worldMatrix;
            BoundingBox = boundingBox;
            RenderQueuePosition = renderQueuePosition;
        }

        public void Render(IGraphics graphics, CameraComponent camera, RenderTexture target)
        {
            switch (graphics.GraphicsAPIs)
            {
                case GraphicsAPIs.OpenGL:
                    RenderOpenGL(graphics, camera, target);
                    break;
                case GraphicsAPIs.D3D11:
                    throw new NotImplementedException();
            }
        }

        void RenderOpenGL(IGraphics graphics, CameraComponent camera, RenderTexture target)
        {
            Mesh.Bind();
            Material.Bind();

            if (Material.Shader.IsTransparent)
            {
                Material.SetTexture("_DepthMap", target.DepthTexture);
                Material.SetTexture("_ColorMap", target.ColorTexture);
            }

            var pMat = camera.GetProjectionMatrix();
            var vMat = camera.GetViewMatrix();
            var vpMat = vMat * pMat;

            Material.SetMatrix("_WorldMatrix", WorldMatrix);
            Material.SetMatrix("_Mvp", WorldMatrix * vpMat);
            Material.SetMatrix("_Vp", vpMat);
            Material.SetMatrix("_Projection", pMat);
            Material.SetMatrix("_Mv", WorldMatrix * vMat);

            if (Mesh.UseTriangles && Mesh.Triangles.Length > 0)
                graphics.DrawElements((uint)Mesh.Triangles.Length);
            else if (Mesh.VertexCount > 0)
                graphics.DrawArrays((uint)Mesh.VertexCount);
        }

        public bool ShouldCull(Frustum frustum)
        {
            if(BoundingBox == null)
                return false;

            return !frustum.Intersects(BoundingBox);
        }
    }
}