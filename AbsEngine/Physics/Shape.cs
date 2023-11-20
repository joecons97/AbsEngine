using Silk.NET.Maths;

namespace AbsEngine.Physics
{
    public interface IShape
    {
        bool Intersects(IShape shape, Vector3D<float> position);

        bool IntersectsForcedOffset(IShape shape, Vector3D<float> position, Vector3D<float> offset);

        bool IntersectsForcedOffset(Vector3D<float> offset, Vector3D<float> position);
    }
}
