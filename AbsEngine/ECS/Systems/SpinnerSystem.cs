using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using Silk.NET.Maths;

namespace AbsEngine;

public class SpinnerSystem : AsyncComponentSystem<TransformComponent>
{
    public SpinnerSystem(Scene scene) : base(scene)
    {
    }

    public override Task OnTickAsync(TransformComponent component, float deltaTime)
    {
        if (component.Entity.GetComponent<CameraComponent>() != null)
            return Task.CompletedTask;

        component.LocalEulerAngles += new Vector3D<float>(0.0f, deltaTime * 90, 0.0f);

        return Task.CompletedTask;
    }
}

public class SpinnerSystemSync : ComponentSystem<TransformComponent>
{
    public SpinnerSystemSync(Scene scene) : base(scene)
    {
    }

    public override void OnTick(TransformComponent component, float deltaTime)
    {
        if (component.Entity.GetComponent<CameraComponent>() != null)
            return;

        component.LocalEulerAngles += new Vector3D<float>(0.0f, deltaTime * 90, 0.0f);
    }
}
