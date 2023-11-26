using AbsEngine;
using AbsEngine.Exceptions;
using AbsGameProject.Blocks;
using AbsGameProject.Terrain;
using Silk.NET.Maths;
using System.Diagnostics;

namespace AbsGameProject.Maths;

public struct RayVoxelOut
{
    public ushort BlockID;
    public Vector3D<float> BlockPosition;
    public Vector3D<float> WorldPosition;
    public TerrainChunkComponent Chunk;

    public Vector3D<float> PlacementPosition;
    public Vector3D<float> PlacementChunk;

    public Vector3D<float> HitNormal;
}

public static class ChunkPhysics
{
    private const float STEP_SIZE = 0.125f;
    /// <summary>
    /// Finds the position of the chunk that the position vector is in
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static Vector3D<float> ToChunkPosition(this Vector3D<float> position)
    {
        Vector3D<float> retVec = new Vector3D<float>();
        retVec.X = MathF.Floor(position.X / TerrainChunkComponent.WIDTH);
        retVec.Y = 0;
        retVec.Z = MathF.Floor(position.Z / TerrainChunkComponent.WIDTH);

        return retVec * TerrainChunkComponent.WIDTH;
    }

    /// <summary>
    /// Finds the position of the vector relative to the chunk it is inside
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static Vector3D<float> ToChunkSpace(this Vector3D<float> position)
    {
        var chunkPos = position.ToChunkPosition();

        return (position - chunkPos);
    }

    public static Vector3D<float> ToChunkSpaceFloored(this Vector3D<float> position)
    {
        var final = ToChunkSpace(position);

        var pos = new Vector3D<float>((float)Math.Floor(final.X), (float)Math.Floor(final.Y), (float)Math.Floor(final.Z));
        return pos;
    }

    public static bool CastVoxel(Vector3D<float> position, Vector3D<float> direction, float distance, out RayVoxelOut output)
    {
        if (Game.Instance == null)
            throw new GameInstanceException();

        RayVoxelOut op = new RayVoxelOut();
        Vector3D<float> curPos = position;
        Vector3D<float> lastPos = new Vector3D<float>();
        float distTravelled = 0;

        while (distTravelled < distance)
        {
            var chunkPos = ToChunkPosition(curPos);
            var pos = (Vector3D<int>)ToChunkSpaceFloored(curPos);

            foreach (var scene in Game.Instance.ActiveScenes)
            {
                var chunk = scene.EntityManager.GetComponents<TerrainChunkComponent>(
                    x =>
                    x.Entity.Transform.LocalPosition.X == chunkPos.X &&
                    x.Entity.Transform.LocalPosition.Z == chunkPos.Z)
                    .FirstOrDefault();

                if (chunk != null && chunk.VoxelData != null)
                {
                    var blockId = chunk.GetBlockId((int)pos.X, (int)pos.Y, (int)pos.Z);
                    var block = BlockRegistry.GetBlock(blockId);

                    if (blockId != 0)
                    {
                        op.BlockID = blockId;
                        op.BlockPosition = (Vector3D<float>)pos;
                        op.WorldPosition = chunkPos + (Vector3D<float>)pos;

                        op.Chunk = chunk;

                        op.PlacementPosition = ToChunkSpaceFloored(lastPos);
                        op.PlacementChunk = ToChunkPosition(lastPos);

                        output = op;
                        return true;
                    }
                }
            }

            lastPos = curPos;
            curPos += direction * STEP_SIZE;
            distTravelled += STEP_SIZE;
        }

        output = default;
        return false;
    }
}
