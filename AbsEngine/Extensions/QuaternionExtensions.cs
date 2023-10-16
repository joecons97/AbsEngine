using Silk.NET.Maths;

namespace AbsEngine.Extensions;

public static class QuaternionExtensions
{
    public static Vector3D<float> ToYawPitchRoll(this Quaternion<float> q)
    {
        Vector3D<float> result = Vector3D<float>.Zero;

        float q2sqr = q.Z * q.Z;
        float t0 = -2.0f * (q2sqr + q.W * q.W) + 1.0f;
        float t1 = +2.0f * (q.Y * q.W + q.X * q.W);
        float t2 = -2.0f * (q.Y * q.W - q.X * q.Z);
        float t3 = +2.0f * (q.Z * q.W + q.X * q.Y);
        float t4 = -2.0f * (q.Y * q.Y + q2sqr) + 1.0f;
        t2 = t2 > 1.0f ? 1.0f : t2;
        t2 = t2 < -1.0f ? -1.0f : t2;

        result.X = MathF.Asin(t2);
        result.Z = MathF.Atan2(t3, t4);
        result.Y = MathF.Atan2(t1, t0);

        return result;
    }
}
