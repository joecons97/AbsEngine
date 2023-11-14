using AbsGameProject.Models;
using AbsGameProject.Textures;

namespace AbsGameProject.Blocks
{
    public class BlockBuilder
    {
        string _name;
        string _id;

        string? _voxelModelFile;

        public BlockBuilder(string name, string id)
        {
            _name = name;
            _id = id;
        }

        public BlockBuilder WithVoxelModel(string voxelModelFile)
        {
            _voxelModelFile = voxelModelFile;
            return this;
        }

        public Block Build()
        {
            VoxelModel? voxelModel = null;
            CullableMesh? cullableMesh = null;

            if (string.IsNullOrEmpty(_voxelModelFile) == false)
            {
                voxelModel = VoxelModel.TryFromFile(_voxelModelFile);
                if (voxelModel == null)
                    throw new Exception("Failed to load voxel model");

                TextureAtlas.InsertVoxelModel(voxelModel);

                cullableMesh = CullableMesh.TryFromVoxelMesh(voxelModel);
                if (cullableMesh == null)
                    throw new Exception("Failed to load mesh from voxel model");
            }

            return new Block()
            {
                Id = _id,
                Name = _name,
                VoxelModelFile = _voxelModelFile,
                VoxelModel = voxelModel,
                Mesh = cullableMesh
            };
        }
    }
}
