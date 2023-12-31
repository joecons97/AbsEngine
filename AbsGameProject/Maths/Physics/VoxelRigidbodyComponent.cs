﻿using AbsEngine.ECS;
using AbsEngine.ECS.Components.Physics;
using Silk.NET.Maths;

namespace AbsGameProject.Maths.Physics;

public class VoxelRigidbodyComponent : Component
{
    public bool IsActive { get; set; } = true;
    public Vector3D<float> Velocity { get; set; }
    public Vector3D<float> AngularVelocity { get; set; }
    public bool UseGravity { get; set; } = true;
    public float Mass { get; set; } = 1.0f;
    public float Drag { get; set; } = 0.0f;
    public IVoxelShape? Shape { get; set; }

    public override void OnStart()
    {
        var collider = (ColliderComponent)Entity.Components.Where(x => x.Key.BaseType == typeof(ColliderComponent)).SelectMany(x => x.Value).First();
        if (collider.Shape is IVoxelShape voxelShape)
            Shape = voxelShape;
    }

    public void AddImpluse(Vector3D<float> impulseForce)
    {
        Velocity += impulseForce / Mass;
    }

    public void AddForce(Vector3D<float> force)
    {
        Velocity += force * Entity.Scene.Game.DeltaTime / Mass;
    }
}
