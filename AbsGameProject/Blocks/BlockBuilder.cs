using AbsGameProject.Maths.Physics;
using AbsGameProject.Models.Meshing;
using AbsGameProject.Textures;

namespace AbsGameProject.Blocks
{
    public class BlockBuilder
    {
        string _name;
        string _id;
        int _opacity;
        int _light;
        bool _transparent;
        bool _cullSelf;
        bool _noCollision;

        string? _voxelModelFile;
        string? _voxelModelFileLod;

        public BlockBuilder(string name, string id)
        {
            _name = name;
            _id = id;
            _opacity = 1;
        }

        public BlockBuilder WithVoxelModel(string voxelModelFile)
        {
            if(!voxelModelFile.StartsWith("Content/Models/Blocks/"))
                voxelModelFile = "Content/Models/Blocks/" + voxelModelFile;

            _voxelModelFile = voxelModelFile;
            return this;
        }

        public BlockBuilder WithVoxelLodModel(string voxelModelLodFile)
        {
            if (!voxelModelLodFile.StartsWith("Content/Models/Blocks/"))
                voxelModelLodFile = "Content/Models/Blocks/" + voxelModelLodFile;

            _voxelModelFileLod = voxelModelLodFile;
            return this;
        }

        public BlockBuilder WithTransparency(bool cullSelf = false)
        {
            _transparent = true;
            _cullSelf = cullSelf;
            return this;
        }

        public BlockBuilder WithLight(int light)
        {
            _light = light;
            return this;
        }

        public BlockBuilder WithOpacity(int opacity)
        {
            _opacity = opacity;
            return this;
        }

        public BlockBuilder WithNoCollision()
        {
            _noCollision = true;
            return this;
        }

        public Block Build()
        {
            VoxelModel? voxelModel = null;
            CullableMesh? cullableMesh = null;
            VoxelBoundingBox[] boundingBoxes = Array.Empty<VoxelBoundingBox>();

            if (string.IsNullOrEmpty(_voxelModelFile) == false)
            {
                voxelModel = VoxelModel.TryFromFile(_voxelModelFile);
                if (voxelModel == null)
                    throw new Exception("Failed to load voxel model");

                TextureAtlas.InsertVoxelModel(voxelModel);

                cullableMesh = CullableMesh.TryFromVoxelMesh(voxelModel);
                if (cullableMesh == null)
                    throw new Exception("Failed to load mesh from voxel model");

                if(_noCollision == false)
                    boundingBoxes = cullableMesh.CollisionBoxes.ToArray();
            }

            VoxelModel? voxelModelLod = null;
            CullableMesh? cullableMeshLod = null;

            if (string.IsNullOrEmpty(_voxelModelFileLod) == false)
            {
                voxelModelLod = VoxelModel.TryFromFile(_voxelModelFileLod);
                if (voxelModelLod == null)
                    throw new Exception("Failed to load voxel model");

                TextureAtlas.InsertVoxelModel(voxelModelLod);

                cullableMeshLod = CullableMesh.TryFromVoxelMesh(voxelModelLod);
                if (cullableMeshLod == null)
                    throw new Exception("Failed to load mesh from voxel model");
            }

            return new Block()
            {
                Id = _id,
                Name = _name,
                VoxelModelFile = _voxelModelFile,
                VoxelModelFileLod = _voxelModelFileLod,
                VoxelModel = voxelModel,
                Mesh = cullableMesh,
                VoxelModelLod = voxelModelLod,
                MeshLod = cullableMeshLod,
                CollisionShapes = boundingBoxes,
                Opacity = _opacity,
                Light = _light,
                IsTransparent = _transparent,
                TransparentCullSelf = _cullSelf
            };
        }
    }
}
