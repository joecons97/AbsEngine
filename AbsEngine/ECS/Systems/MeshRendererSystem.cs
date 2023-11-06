using AbsEngine.ECS.Components;
using AbsEngine.Maths;
using AbsEngine.Rendering;
using Silk.NET.Maths;

namespace AbsEngine.ECS.Systems;

public class MeshRendererSystem : ComponentSystem<MeshRendererComponent>
{
    public MeshRendererSystem(Scene scene) : base(scene)
    {
    }

    public override void OnTick(MeshRendererComponent component, float deltaTime)
    {
        if(component.IsEnabled == false || component.Material == null || component.Mesh == null) 
            return;

        var trans = component.Entity.Transform;

        var world = trans.WorldMatrix;

        Renderer.Render(component.Mesh, component.Material, world);
    }
}
