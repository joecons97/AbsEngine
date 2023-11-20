using AbsEngine.ECS;

namespace AbsGameProject.Physics;

public class VoxelRigidbodySimulationSystem : AsyncComponentSystem<VoxelRigidbodyComponent>
{
    public static float Gravity = -9.81f;

    protected override float? FixedTimeStep => 0.03333f;

    public VoxelRigidbodySimulationSystem(Scene scene) : base(scene)
    {
    }

    public override Task OnTickAsync(VoxelRigidbodyComponent component, float deltaTime)
    {
        if (component.IsActive == false)
            return Task.CompletedTask;

        component.Velocity += new Silk.NET.Maths.Vector3D<float>(0, Gravity * deltaTime, 0);

        component.Velocity *= 1 / (1 + component.Drag * deltaTime);

        component.Entity.Transform.LocalPosition += component.Velocity * deltaTime;

        return Task.CompletedTask;
    }
}
