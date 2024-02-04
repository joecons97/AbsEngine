using AbsEngine.Physics;
using AbsEngine.Rendering;

namespace AbsEngine.ECS.Components;

public class MeshRendererComponent : Component
{
    public BoundingBox? BoundingBox { get; set; }
    public Mesh? Mesh { get; set; }
    public Material? Material { get; set; }

    public MeshRendererComponent(Mesh mesh, Material material)
    {
        Mesh = mesh;
        Material = material;
    }

    public MeshRendererComponent(Mesh mesh, Material material, BoundingBox? boundingBox)
    {
        Mesh = mesh;
        Material = material;
        BoundingBox = boundingBox;
    }

    public MeshRendererComponent()
    {
    }
}
