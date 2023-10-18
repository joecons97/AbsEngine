using AbsEngine.Rendering;

namespace AbsEngine.ECS.Components;

public class MeshRendererComponent : Component
{
    public Mesh? Mesh { get; set; }
    public Material? Material { get; set; }

    public MeshRendererComponent(Mesh mesh, Material material)
    {
        Mesh = mesh;
        Material = material;
    }

    public MeshRendererComponent()
    {
    }
}
