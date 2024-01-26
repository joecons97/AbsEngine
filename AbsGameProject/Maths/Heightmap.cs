using AbsGameProject.Components.Terrain;
using AbsGameProject.Extensions;
using AbsGameProject.Maths.Noise;
using Assimp;
using Silk.NET.Maths;
using System.Diagnostics;

namespace AbsGameProject.Maths;

public static class Heightmap
{
    private static FastNoiseLite noise;
    private static float noiseSize = 5f;
    private static float amplitude = 70;

    private static int octaves = 12;
    private static float lacunarity = 2;
    private static float persistence = 0.5f;


    static Heightmap()
    {
        noise = new FastNoiseLite();
    }

    public static float GetHeightAt(int x, int z)
    {
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
        var continent = (noise.GetNoise(
            new Vector2D<float>(x, z),
            noiseSize,
            octaves,
            persistence,
            lacunarity) + 1) / 2;

        continent = MathF.Pow(continent, 1.5f);
        continent *= amplitude / 1.25f;
        continent += 10;

        var mountainStrength = (noise.GetNoise(
            new Vector2D<float>(x, z),
            noiseSize * 6,
            1,
            persistence,
        lacunarity) + 1) / 2;

        var mountains = (noise.GetNoiseRidged(
            new Vector2D<float>(x, z),
            noiseSize * 3,
            8,
            persistence,
            lacunarity) + 1) / 2;

        mountains = MathF.Pow(mountains, 3);
        mountains *= amplitude * 2;
        mountains = mountains * mountainStrength;

        return (int)MathF.Max(mountains, continent);
    }
}
