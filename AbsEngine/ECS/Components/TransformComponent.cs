﻿using AbsEngine.Extensions;
using AbsEngine.Maths;
using Silk.NET.Maths;

namespace AbsEngine.ECS.Components;

public class TransformComponent : Component
{
    TransformComponent? parent;
    List<TransformComponent> children = new List<TransformComponent>();
    Vector3D<float> eulerAngles;
    Quaternion<float> quaternion = Quaternion<float>.Identity;

    Vector3D<float> _worldScale;
    Quaternion<float> _worldRotation;
    Vector3D<float> _worldPosition;


    public TransformComponent? Parent 
    {
        get => parent; 
        set
        {
            if(parent != null && parent.children.Contains(this))
                parent.children.Remove(this);

            parent = value;

            if(parent != null)
                parent.children.Add(this);  
        }
    }

    public Vector3D<float> Position 
    { 
        get => _worldPosition;
        set 
        {
            var diff = parent != null ? parent._worldPosition : Vector3D<float>.Zero;

            LocalPosition = value - diff;
        }
    }

    public Matrix4X4<float> WorldMatrix
    {
        get
        {
            var local = Matrix4X4.CreateScale(LocalScale) *
                            Matrix4X4.CreateFromQuaternion(LocalRotation) *
                            Matrix4X4.CreateTranslation(LocalPosition);

            if (Parent != null)
            {
                local = local * Parent.WorldMatrix;
            }

            Matrix4X4.Decompose<float>(local, out _worldScale, out _worldRotation, out _worldPosition);

            return local;
        }
    }

    public Vector3D<float> LocalPosition { get; set; }

    public Vector3D<float> LocalEulerAngles
    {
        get => eulerAngles;
        set
        {
            eulerAngles = value;

            quaternion = Quaternion<float>.CreateFromYawPitchRoll(
                eulerAngles.Y * AbsMaths.DEG_2_RAD,
                eulerAngles.X * AbsMaths.DEG_2_RAD,
                eulerAngles.Z * AbsMaths.DEG_2_RAD);
        }
    }

    public Quaternion<float> LocalRotation
    {
        get => quaternion;
        set
        {
            quaternion = value;

            eulerAngles = quaternion.ToYawPitchRoll();
        }
    }

    public Vector3D<float> LocalScale { get; set; } = Vector3D<float>.One;

    public Vector3D<float> Forward
    {
        get
        {
            return Vector3D.Normalize(Vector3D.Transform(-Vector3D<float>.UnitZ, _worldRotation));
        }
    }

    public Vector3D<float> Right
    {
        get
        {
            return Vector3D.Normalize(Vector3D.Transform(-Vector3D<float>.UnitX, _worldRotation));
        }
    }

    public Vector3D<float> Up
    {
        get
        {
            return Vector3D.Normalize(Vector3D.Cross(Right, Forward));
        }
    }

    public TransformComponent? GetChild(string name)
    {
        return children.FirstOrDefault(x => x.Entity.Name == name); 
    }
}
