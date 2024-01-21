using AbsEngine.ECS;
using AbsGameProject.Components.Physics;
using Silk.NET.Maths;

namespace AbsGameProject.Systems.Physics;

public class VoxelRigidbodySimulationSystem : ComponentSystem<VoxelRigidbodyComponent>
{
    public static float Gravity = -25f;

    protected override float? FixedTimeStep => 0.0166666667f;

    protected override bool UseJobSystem => false;

    public VoxelRigidbodySimulationSystem(Scene scene) : base(scene)
    {
    }

    public override void OnTick(VoxelRigidbodyComponent component, float deltaTime)
    {
        if (component.IsActive == false)
            return;

        var normVelocity = Vector3D.Normalize(component.Velocity);

        if (component.UseGravity)
            component.Velocity += new Vector3D<float>(0, Gravity * deltaTime, 0);

        if (component.Shape != null)
        {
            if (component.Velocity.X != 0)
            {
                var sign = MathF.Sign(normVelocity.X);
                if (component.Shape.IntersectsWorldDirectional(component, new Vector3D<float>(sign * 0.1f, 0.1f, 0)))
                    component.Velocity = new Vector3D<float>(0, component.Velocity.Y, component.Velocity.Z);
            }

            if (component.Velocity.Z != 0)
            {
                var sign = MathF.Sign(normVelocity.Z);
                if (component.Shape.IntersectsWorldDirectional(component, new Vector3D<float>(0, 0.1f, sign * 0.1f)))
                    component.Velocity = new Vector3D<float>(component.Velocity.X, component.Velocity.Y, 0);
            }


            if (component.Velocity.Y > 0)
            {
                if (component.Shape.IntersectsWorldDirectional(component, new Vector3D<float>(0, .1f, 0)))
                    component.Velocity = new Vector3D<float>(component.Velocity.X, 0, component.Velocity.Z);
            }
            else if (component.Velocity.Y < 0)
            {
                if (component.Shape.IntersectsWorldDirectional(component, new Vector3D<float>(0, -.1f, 0)))
                {
                    component.Velocity = new Vector3D<float>(component.Velocity.X, 0, component.Velocity.Z);
                    component.Entity.Transform.LocalPosition = new Vector3D<float>(component.Entity.Transform.LocalPosition.X,
                        (float)Math.Round(component.Entity.Transform.LocalPosition.Y), component.Entity.Transform.LocalPosition.Z);
                }
            }
        }

        component.Velocity *= 1 / (1 + component.Drag * deltaTime);

        component.Entity.Transform.LocalPosition += component.Velocity * deltaTime;
    }
}
