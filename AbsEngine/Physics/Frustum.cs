using Silk.NET.Maths;

namespace AbsEngine.Physics;

public class Frustum
{
    public const int PLANE_COUNT = 6;
    public const int CORNER_COUNT = 8;

    private Matrix4X4<float> _matrix;
    private readonly Vector3D<float>[] _corners = new Vector3D<float>[CORNER_COUNT];
    private readonly Plane<float>[] _planes = new Plane<float>[PLANE_COUNT];

    public Matrix4X4<float> Matrix
    {
        get { return _matrix; }
        set
        {
            _matrix = value;
            CreatePlanes();    // FIXME: The odds are the planes will be used a lot more often than the matrix
            CreateCorners();   // is updated, so this should help performance. I hope ;)
        }
    }

    public Plane<float> Near
    {
        get { return _planes[0]; }
    }

    public Plane<float> Far
    {
        get { return _planes[1]; }
    }

    public Plane<float> Left
    {
        get { return _planes[2]; }
    }

    public Plane<float> Right
    {
        get { return _planes[3]; }
    }

    public Plane<float> Top
    {
        get { return _planes[4]; }
    }

    public Plane<float> Bottom
    {
        get { return _planes[5]; }
    }

    public Frustum(Matrix4X4<float> value)
    {
        _matrix = value;
        CreatePlanes();
        CreateCorners();
    }

    private void CreateCorners()
    {
        IntersectionPoint(_planes[0], _planes[2], _planes[4], out _corners[0]);
        IntersectionPoint(_planes[0], _planes[3], _planes[4], out _corners[1]);
        IntersectionPoint(_planes[0], _planes[3], _planes[5], out _corners[2]);
        IntersectionPoint(_planes[0], _planes[2], _planes[5], out _corners[3]);
        IntersectionPoint(_planes[1], _planes[2], _planes[4], out _corners[4]);
        IntersectionPoint(_planes[1], _planes[3], _planes[4], out _corners[5]);
        IntersectionPoint(_planes[1], _planes[3], _planes[5], out _corners[6]);
        IntersectionPoint(_planes[1], _planes[2], _planes[5], out _corners[7]);
    }

    private void CreatePlanes()
    {
        _planes[0] = new Plane<float>(-_matrix.M13, -_matrix.M23, -_matrix.M33, -_matrix.M43);
        _planes[1] = new Plane<float>(_matrix.M13 - _matrix.M14, _matrix.M23 - _matrix.M24, _matrix.M33 - _matrix.M34, _matrix.M43 - _matrix.M44);
        _planes[2] = new Plane<float>(-_matrix.M14 - _matrix.M11, -_matrix.M24 - _matrix.M21, -_matrix.M34 - _matrix.M31, -_matrix.M44 - _matrix.M41);
        _planes[3] = new Plane<float>(_matrix.M11 - _matrix.M14, _matrix.M21 - _matrix.M24, _matrix.M31 - _matrix.M34, _matrix.M41 - _matrix.M44);
        _planes[4] = new Plane<float>(_matrix.M12 - _matrix.M14, _matrix.M22 - _matrix.M24, _matrix.M32 - _matrix.M34, _matrix.M42 - _matrix.M44);
        _planes[5] = new Plane<float>(-_matrix.M14 - _matrix.M12, -_matrix.M24 - _matrix.M22, -_matrix.M34 - _matrix.M32, -_matrix.M44 - _matrix.M42);

        NormalizePlane(ref _planes[0]);
        NormalizePlane(ref _planes[1]);
        NormalizePlane(ref _planes[2]);
        NormalizePlane(ref _planes[3]);
        NormalizePlane(ref _planes[4]);
        NormalizePlane(ref _planes[5]);
    }

    private void IntersectionPoint(Plane<float> a, Plane<float> b, Plane<float> c, out Vector3D<float> result)
    {
        Vector3D<float> v1, v2, v3;
        Vector3D<float> cross;

        cross = Vector3D.Cross(b.Normal, c.Normal);

        float f;
        f = Vector3D.Dot(a.Normal, cross);
        f *= -1.0f;

        cross = Vector3D.Cross(b.Normal, c.Normal);
        v1 = Vector3D.Multiply(cross, a.Distance);


        cross = Vector3D.Cross(c.Normal, a.Normal);
        v2 = Vector3D.Multiply(cross, b.Distance);


        cross = Vector3D.Cross(a.Normal, b.Normal);
        v3 = Vector3D.Multiply(cross, c.Distance);

        result.X = (v1.X + v2.X + v3.X) / f;
        result.Y = (v1.Y + v2.Y + v3.Y) / f;
        result.Z = (v1.Z + v2.Z + v3.Z) / f;
    }

    private void NormalizePlane(ref Plane<float> p)
    {
        float factor = 1f / p.Normal.Length;
        p.Normal.X *= factor;
        p.Normal.Y *= factor;
        p.Normal.Z *= factor;
        p.Distance *= factor;
    }

    public void Contains(BoundingBox box, out ContainmentType result)
    {
        var intersects = false;
        for (var i = 0; i < PLANE_COUNT; ++i)
        {
            var planeIntersectionType = default(PlaneIntersectionType);
            box.Intersects(this._planes[i], out planeIntersectionType);
            switch (planeIntersectionType)
            {
                case PlaneIntersectionType.Front:
                    result = ContainmentType.Disjoint;
                    return;
                case PlaneIntersectionType.Intersecting:
                    intersects = true;
                    break;
            }
        }
        result = intersects ? ContainmentType.Intersects : ContainmentType.Contains;
    }

    public void Intersects(BoundingBox box, out bool result)
    {
        var containment = default(ContainmentType);
        Contains(box, out containment);
        result = containment != ContainmentType.Disjoint;
    }

    public bool Intersects(BoundingBox box)
    {
        var result = false;
        Intersects(box, out result);
        return result;
    }
}
