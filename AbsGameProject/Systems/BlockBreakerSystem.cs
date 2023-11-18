using AbsEngine;
using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsGameProject.Maths;

namespace AbsGameProject.Systems;

public class BlockBreakerSystem : AbsEngine.ECS.System
{
    private CameraComponent _mainCamera = null!;
    private Entity cubeEnt;

    public BlockBreakerSystem(Scene scene) : base(scene)
    {
    }

    public override void OnStart()
    {
        _mainCamera = Scene.EntityManager.GetComponents<CameraComponent>(x => x.IsMainCamera).First();
        cubeEnt = Scene.EntityManager.GetComponents<NameComponent>(x => x.Name == "Test Forward Cube").First().Entity;

        Game.Instance.InputContext.Keyboards[0].KeyDown += BlockBreakerSystem_KeyDown;
    }

    private void BlockBreakerSystem_KeyDown(Silk.NET.Input.IKeyboard arg1, Silk.NET.Input.Key arg2, int arg3)
    {
        if(arg2 == Silk.NET.Input.Key.Space)
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
