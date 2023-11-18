using AbsEngine;
using AbsEngine.ECS.Components;
using AbsEngine.Exceptions;
using AbsEngine.Rendering;
using AbsGameProject.Blocks;
using AbsGameProject.Models;
using AbsGameProject.Terrain;
using Silk.NET.Maths;

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
    static Mesh debugMesh;
    static Material debugMaterial;
    static Material debugMaterialRed;

    static ChunkPhysics()
    {
        var vox = VoxelModel.TryFromFile("Content/Models/Blocks/Dirt.json");
        var cullableMesh = CullableMesh.TryFromVoxelMesh(vox);
        var mesh = new Mesh();
        mesh.UseTriangles = false;
        mesh.Positions = cullableMesh.Faces.Select(x => x.Value.Positions).SelectMany(x => x).ToArray();

        mesh.Build();

        debugMesh = mesh;
        debugMaterial = new Material("NewSyntax");
        debugMaterial.SetVector("Colour", new Vector4D<float>(0, 1, 0, 1));

        debugMaterialRed = new Material("NewSyntax");
        debugMaterialRed.SetVector("Colour", new Vector4D<float>(1, 0, 0, 1));
    }

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

    public static Vector3D<float> ToChunkSpaceRounded(this Vector3D<float> position, Vector3D<float> viewPos)
    {
        var final = ToChunkSpace(position);

        var d = Vector3D.Dot(viewPos, position);
        if (d >= 0)
        {
            var pos = new Vector3D<float>((float)Math.Ceiling(final.X), (float)Math.Ceiling(final.Y), (float)Math.Ceiling(final.Z));
            return pos;
        }
        else
        {
            var pos = new Vector3D<float>((float)Math.Floor(final.X), (float)Math.Floor(final.Y), (float)Math.Floor(final.Z));
            return pos;
        }
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
            var pos = (Vector3D<int>)ToChunkSpaceRounded(curPos, position);

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

                    if (SceneCameraComponent.IsInSceneView)
                    {
                        Renderer.Render(debugMesh, debugMaterialRed,
                            Matrix4X4.CreateScale(Vector3D<float>.One * .25f) * Matrix4X4.CreateTranslation(chunkPos + (Vector3D<float>)pos));

                        Renderer.Render(debugMesh, debugMaterialRed,
                            Matrix4X4.CreateScale(Vector3D<float>.One * .25f) * Matrix4X4.CreateTranslation(curPos));

                        Renderer.Render(debugMesh, debugMaterial,
                            Matrix4X4.CreateScale(Vector3D<float>.One) * Matrix4X4.CreateTranslation(op.WorldPosition));
                    }

                    if (block != null)
                    {
                        op.BlockID = blockId;
                        op.BlockPosition = (Vector3D<float>)pos;
                        op.WorldPosition = chunkPos + (Vector3D<float>)pos;

                        op.Chunk = chunk;

                        op.PlacementPosition = ToChunkSpaceRounded(lastPos, position);
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
