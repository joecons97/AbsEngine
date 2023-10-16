using AbsEngine.Rendering;

namespace AbsEngine.ECS.Components;

public class MeshRendererComponent : Component
{
    public Mesh? Mesh { get; set; }
    public Shader? Shader { get; set; }

    public MeshRendererComponent(Mesh mesh, Shader shader)
    {
        Mesh = mesh;
        Shader = shader;
    }

    public MeshRendererComponent()
    {
    }
}
