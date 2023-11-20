using AbsEngine.Physics;
using Silk.NET.Maths;

namespace AbsEngine.ECS.Components.Physics;

public class BoxColliderComponent : ColliderComponent
{
    private BoundingBox _boundingBox = new BoundingBox(0, 1, 0, 1, 0, 1);

    public override IShape Shape => _boundingBox;

    public Vector3D<float> Min { get => _boundingBox.Min; set => _boundingBox.Min = value; }

    public Vector3D<float> Max { get => _boundingBox.Max; set => _boundingBox.Max = value; }

    public Vector3D<float> Size => _boundingBox.Size;
}
