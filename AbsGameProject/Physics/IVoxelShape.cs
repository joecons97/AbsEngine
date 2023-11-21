using AbsEngine.Physics;
using Silk.NET.Maths;

namespace AbsGameProject.Physics;

public interface IVoxelShape : IShape
{
    public bool IntersectsWorld(VoxelRigidbodyComponent body)
    {
        return IntersectsWorldDirectional(body, Vector3D<float>.Zero);
    }

    bool IntersectsWorldDirectional(VoxelRigidbodyComponent body, Vector3D<float> direction);
}
