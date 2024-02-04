namespace AbsEngine.Maths;

public static class AbsMaths
{
    public const float DEG_2_RAD = MathF.PI / 180f;
    public const float RAD_2_DEG = 180f / MathF.PI;

    public static float InverseLerp(float a, float b, float t)
        => (t - a) / (b - a);
}
