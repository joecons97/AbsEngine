using AbsEngine.Physics;
using AbsGameProject.Maths.Physics;
using AbsGameProject.Models;

namespace AbsGameProject.Blocks;

public class Block
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";

    public string? VoxelModelFile { get; init; } = "";
    
    public VoxelModel? VoxelModel { get; init; }
    public CullableMesh? Mesh { get;init; }

    public int Opacity { get; init; }
    public int Light { get; init; }

    public bool IsTransparent { get; init; }
    public bool TransparentCullSelf { get; init; }

    public IVoxelShape[] CollisionShapes { get; init; } 

    public static BlockBuilder New(string id, string name)
    {
        return new BlockBuilder(name, id);
    }
}
