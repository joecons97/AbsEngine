using AbsGameProject.Blocks;
using AbsGameProject.Components.Terrain;

namespace AbsGameProject.Structures;

internal class TreeDecorator : Decorator
{
    public Block Trunk { get; }
    public Block Leaves { get; }
    public int TrunkHeight { get; }
    public int RandomHeightOffset { get; }
    public int LeavesRadius { get; }

    Random random;

    public TreeDecorator(Block trunk, Block leaves, int trunkHeight, int leavesRadius, int randomHeightOffset)
    {
        Trunk = trunk;
        Leaves = leaves;
        TrunkHeight = trunkHeight;
        LeavesRadius = leavesRadius;
        RandomHeightOffset = randomHeightOffset;

        random = new Random();
    }

    public override float GetRadius()
    {
        return 5;
    }

    public override Task DecorateAtAsync(TerrainChunkComponent chunk, int x, int y, int z)
    {
        var height = TrunkHeight + random.Next(RandomHeightOffset);
        int leaves = LeavesRadius;
        var leavesHeight = height - leaves;

        for (int ly = 0; ly <= leaves; ly++)
        {
            int currentHeight = (ly == leaves) ? leavesHeight + 2 : leavesHeight + ly;

            for (int lx = -leaves; lx <= leaves; lx++)
            {
                for (int lz = -leaves; lz <= leaves; lz++)
                {
                    if (ly == leaves && (lx == -leaves || lz == -leaves || lx == leaves || lz == leaves))
                        continue;

                    chunk.SetBlock(x + lx, y + currentHeight, z + lz, Leaves, logChange: false);
                }
            }
        }

        chunk.SetBlock(x, y + leavesHeight + 1 + leaves, z, Leaves, logChange: false);
        chunk.SetBlock(x + 1, y + leavesHeight + 1 + leaves, z, Leaves, logChange: false);
        chunk.SetBlock(x - 1, y + leavesHeight + 1 + leaves, z, Leaves, logChange: false);
        chunk.SetBlock(x, y + leavesHeight + 1 + leaves, z + 1, Leaves, logChange: false);
        chunk.SetBlock(x, y + leavesHeight + 1 + leaves, z - 1, Leaves, logChange: false);

        for (int i = 0; i < height; i++)
        {
            chunk.SetBlock(x, y + i, z, Trunk, logChange: false);
        }

        return Task.CompletedTask;
    }
}
