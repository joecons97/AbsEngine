using AbsEngine;
using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsGameProject.Maths;

namespace AbsGameProject.Systems;

public class BlockBreakerSystem : AbsEngine.ECS.System
{
    private CameraComponent _mainCamera = null!;

    public BlockBreakerSystem(Scene scene) : base(scene)
    {
    }

    public override void OnStart()
    {
        _mainCamera = Scene.EntityManager.GetComponents<CameraComponent>(x => x.IsMainCamera).First();

        Scene.Game.InputContext.Mice.First().Click += BlockBreakerSystem_Click;
    }

    private void BlockBreakerSystem_Click(Silk.NET.Input.IMouse mouse, Silk.NET.Input.MouseButton btn, System.Numerics.Vector2 pos)
    {
        if(btn == Silk.NET.Input.MouseButton.Left)
        {
            if (ChunkPhysics.CastVoxel(_mainCamera.Entity.Transform.LocalPosition, _mainCamera.Entity.Transform.Forward, 5, out var output))
            {
                output.Chunk.SetBlock((int)output.BlockPosition.X, (int)output.BlockPosition.Y, (int)output.BlockPosition.Z, null);

                output.Chunk.RebuildMesh();
            }
        }
    }

    public override void Tick(float deltaTime)
    {
        //if (ChunkPhysics.CastVoxel(_mainCamera.Entity.Transform.LocalPosition, _mainCamera.Entity.Transform.Forward, 5, out var output))
        //{
        //    cubeEnt.Transform.LocalPosition = output.WorldPosition;
        //}
    }

}
