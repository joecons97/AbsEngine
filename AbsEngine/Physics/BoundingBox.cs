using Silk.NET.Maths;

namespace AbsEngine.Physics;

public class BoundingBox : IShape
{
    public Vector3D<float> Min { get; set; }
    public Vector3D<float> Max { get; set; }

    public Vector3D<float> Size
    {
        get => Max - Min;
    }

    public BoundingBox(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
    {
        Min = new Vector3D<float>(minX, minY, minZ);
        Max = new Vector3D<float>(maxX, maxY, maxZ);
    }

    public BoundingBox Transform(Vector3D<float> translation, Vector3D<float> scale)
    {
        return new BoundingBox(Min.X + translation.X * scale.X,
                                Max.X + translation.X * scale.X,
                                Min.Y + translation.Y * scale.Y,
                                Max.Y + translation.Y * scale.Y,
                                Min.Z + translation.Z * scale.Z,
                                Max.Z + translation.Z * scale.Z);
    }

    public bool Intersects(IShape shape, Vector3D<float> position)
    {
        return IntersectsForcedOffset(shape, position, Vector3D<float>.Zero);
    }

    public bool IntersectsForcedOffset(IShape shape, Vector3D<float> position, Vector3D<float> offset)
    {
        if (shape is BoundingBox box)
        {
            return (offset.X + Min.X <= position.X + box.Max.X &&
                    offset.X + Max.X >= position.X + box.Min.X &&
                    offset.Y + Min.Y <= position.Y + box.Max.Y &&
                    offset.Y + Max.Y >= position.Y + box.Min.Y &&
                    offset.Z + Min.Z <= position.Z + box.Max.Z &&
                    offset.Z + Max.Z >= position.Z + box.Min.Z);
        }

        return false;
    }

    public bool IntersectsForcedOffset(Vector3D<float> offset, Vector3D<float> position)
    {
        return (offset.X + Min.X <= position.X &&
                offset.X + Max.X >= position.X &&
                offset.Y + Min.Y <= position.Y &&
                offset.Y + Max.Y >= position.Y &&
                offset.Z + Min.Z <= position.Z &&
                offset.Z + Max.Z >= position.Z);
    }
}
