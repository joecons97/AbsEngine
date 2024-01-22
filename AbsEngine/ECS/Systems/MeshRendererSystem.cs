using AbsEngine.ECS.Components;
using AbsEngine.Rendering;

namespace AbsEngine.ECS.Systems;

public class MeshRendererSystem : ComponentSystem<MeshRendererComponent>
{
    public override bool UseJobSystem => false;

    public MeshRendererSystem(Scene scene) : base(scene)
    {
    }

    public override void OnTick(MeshRendererComponent component, float deltaTime)
    {
        if (component.Material == null || component.Mesh == null)
            return;

        var trans = component.Entity.Transform;

        var world = trans.WorldMatrix;

        Renderer.Render(component.Mesh, component.Material, world, component.BoundingBox);
    }
}
