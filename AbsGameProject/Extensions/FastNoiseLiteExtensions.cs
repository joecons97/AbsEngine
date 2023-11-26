using AbsGameProject.Maths.Noise;
using Silk.NET.Maths;

namespace AbsGameProject.Extensions;

public static class FastNoiseLiteExtensions
{
    public static float GetNoise(this FastNoiseLite noise, Vector2D<float> pos, float scale, int octaves, float persistance, float lacunarity)
    {
        float amplitude = 1;
        float frequency = 1;
        float noiseHeight = 0;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = pos.X / scale * frequency;
            float sampleY = pos.Y / scale * frequency;

            float value = noise.GetNoise(sampleX, sampleY);
            noiseHeight += value * amplitude;

            amplitude *= persistance;
            frequency *= lacunarity;
        }

        return noiseHeight;
    }
}
