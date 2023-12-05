using AbsGameProject.Components.Terrain;

namespace AbsGameProject.Structures;

public abstract class Decorator
{
    public abstract Task DecorateAtAsync(TerrainChunkComponent chunk, int x, int y, int z);
    public abstract float GetRadius();
}
