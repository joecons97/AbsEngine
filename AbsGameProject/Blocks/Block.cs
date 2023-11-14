using AbsGameProject.Models;

namespace AbsGameProject.Blocks;

public class Block
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";

    public string? VoxelModelFile { get; init; } = "";
    
    public VoxelModel? VoxelModel { get; init; }
    public CullableMesh? Mesh { get;init; }

    public static BlockBuilder New(string name, string id)
    {
        return new BlockBuilder(name, id);
    }
}
