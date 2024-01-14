using AbsEngine;
using AbsEngine.Physics;
using AbsGameProject.Blocks;
using AbsGameProject.Components.Physics;
using AbsGameProject.Components.Terrain;
using Silk.NET.Maths;

namespace AbsGameProject.Maths.Physics;

public class VoxelBoundingBox : BoundingBox, IVoxelShape
{
    public VoxelBoundingBox(float minX, float maxX, float minY, float maxY, float minZ, float maxZ) : base(minX, maxX, minY, maxY, minZ, maxZ)
    {
    }

    public bool IntersectsWorldDirectional(VoxelRigidbodyComponent body, Vector3D<float> direction)
    {
        //Bottom
        bool BottomBackLeft()
        {
            var pos = body.Entity.Transform.LocalPosition + new Vector3D<float>(Min.X, Min.Y, Min.Z) + direction;
            var chunkPosition = (pos).ToChunkPosition();
            var posInChunk = (pos).ToChunkSpaceFloored();
            return DoPos(chunkPosition, posInChunk);
        }
        bool BottomBackRight()
        {
            var pos = body.Entity.Transform.LocalPosition + new Vector3D<float>(Max.X, Min.Y, Min.Z) + direction;
            var chunkPosition = (pos).ToChunkPosition();
            var posInChunk = (pos).ToChunkSpaceFloored();
            return DoPos(chunkPosition, posInChunk);
        }
        bool BottomFrontLeft()
        {
            var pos = body.Entity.Transform.LocalPosition + new Vector3D<float>(Min.X, Min.Y, Max.Z) + direction;
            var chunkPosition = (pos).ToChunkPosition();
            var posInChunk = (pos).ToChunkSpaceFloored();
            return DoPos(chunkPosition, posInChunk);
        }
        bool BottomFrontRight()
        {
            var pos = body.Entity.Transform.LocalPosition + new Vector3D<float>(Max.X, Min.Y, Max.Z) + direction;
            var chunkPosition = (pos).ToChunkPosition();
            var posInChunk = (pos).ToChunkSpaceFloored();
            return DoPos(chunkPosition, posInChunk);
        }

        //Top
        bool TopBackLeft()
        {
            var pos = body.Entity.Transform.LocalPosition + new Vector3D<float>(Min.X, Max.Y, Min.Z) + direction;
            var chunkPosition = (pos).ToChunkPosition();
            var posInChunk = (pos).ToChunkSpaceFloored();
            return DoPos(chunkPosition, posInChunk);
        }
        bool TopBackRight()
        {
            var pos = body.Entity.Transform.LocalPosition + new Vector3D<float>(Max.X, Max.Y, Min.Z) + direction;
            var chunkPosition = (pos).ToChunkPosition();
            var posInChunk = (pos).ToChunkSpaceFloored();
            return DoPos(chunkPosition, posInChunk);
        }
        bool TopFrontLeft()
        {
            var pos = body.Entity.Transform.LocalPosition + new Vector3D<float>(Min.X, Max.Y, Max.Z) + direction;
            var chunkPosition = (pos).ToChunkPosition();
            var posInChunk = (pos).ToChunkSpaceFloored();
            return DoPos(chunkPosition, posInChunk);
        }
        bool TopFrontRight()
        {
            var pos = body.Entity.Transform.LocalPosition + new Vector3D<float>(Max.X, Max.Y, Max.Z) + direction;
            var chunkPosition = (pos).ToChunkPosition();
            var posInChunk = (pos).ToChunkSpaceFloored();
            return DoPos(chunkPosition, posInChunk);
        }

        bool DoPos(Vector3D<float> chunkPosition, Vector3D<float> posInChunk)
        {
            var chunk = Game.Instance?.ActiveScenes
                .SelectMany(x =>
                    x.EntityManager.GetComponents<TerrainChunkComponent>(x =>
                        x.Entity.Transform.LocalPosition.X == chunkPosition.X && x.Entity.Transform.LocalPosition.Z == chunkPosition.Z))
                .FirstOrDefault();

            if (chunk != null)
            {
                ushort id = chunk.GetBlockId((int)(posInChunk.X), (int)(posInChunk.Y), (int)(posInChunk.Z));
                if (id == 0)
                    return false;

                Block block = BlockRegistry.GetBlock(id);
                if (block.CollisionShapes != null)
                {
                    return block.CollisionShapes.Any(x =>
                    {
                        var blockPos = chunkPosition + posInChunk;
                        return x.IntersectsForcedOffset(this, body.Entity.Transform.LocalPosition, blockPos - direction);
                    });
                }
            }

            return false;
        }

        if (!(direction.X == 0 && direction.Z == 0 && direction.Y > 0))
        {
            if (BottomBackLeft())
                return true;
            if (BottomBackRight())
                return true;
            if (BottomFrontLeft())
                return true;
            if (BottomFrontRight())
                return true;
        }

        if (direction.Y >= 0)
        {
            direction.Y = -direction.Y;

            if (TopBackLeft())
                return true;
            if (TopBackRight())
                return true;
            if (TopFrontLeft())
                return true;
            if (TopFrontRight())
                return true;
        }


        return false;
    }
}
