using AbsEngine.Physics;
using AbsGameProject.Maths.Physics;
using AbsGameProject.Models.Meshing;

namespace AbsGameProject.Blocks;

public class Block
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";

    public string? VoxelModelFile { get; init; } = "";
    public string? VoxelModelFileLod { get; init; } = "";

    public VoxelModel? VoxelModel { get; init; }
    public VoxelModel? VoxelModelLod { get; init; }
    public CullableMesh? Mesh { get;init; }
    public CullableMesh? MeshLod { get; init; }

    public int Opacity { get; init; }
    public int Light { get; init; }

    public bool IsTransparent { get; init; }
    public bool TransparentCullSelf { get; init; }

    public IVoxelShape[] CollisionShapes { get; init; } = Array.Empty<IVoxelShape>();

    public static BlockBuilder New(string id, string name)
    {
        return new BlockBuilder(name, id);
    }

    public override bool Equals(object? obj)
    {
        var other = obj as Block;

        if(other == null) return false;

        return other.Id == Id;
    }
}
