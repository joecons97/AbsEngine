﻿using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsEngine.IO;
using AbsEngine.Rendering;
using AbsGameProject.Maths;
using AbsGameProject.Player;
using Silk.NET.Maths;

namespace AbsGameProject.Systems;

public class BlockBreakerSystem : AbsEngine.ECS.System
{
    private TransformComponent _mainCamera = null!;
    private Entity debugCube;

    public BlockBreakerSystem(Scene scene) : base(scene)
    {
        Mesh mesh = MeshLoader.LoadMesh("Content/Models/Cube.obj");
        mesh.Build();

        debugCube = Scene.EntityManager.CreateEntity("Debug Cube");
        debugCube.Transform.LocalScale = new Vector3D<float>(-0.5125f, 0.5125f, -0.5125f);

        var r = debugCube.AddComponent<MeshRendererComponent>();
        r.Mesh = mesh;
        r.Material = new Material("BlockSelection");
    }

    public override void OnStart()
    {
        _mainCamera = Scene.EntityManager.GetComponents<PlayerControllerComponent>().First().CameraEntityTransform!;

        Scene.Game.InputContext.Mice.First().MouseDown += BlockBreakerSystem_MouseDown; ;
    }

    private void BlockBreakerSystem_MouseDown(Silk.NET.Input.IMouse mouse, Silk.NET.Input.MouseButton btn)
    {
        if (btn == Silk.NET.Input.MouseButton.Left)
        {
            if (ChunkPhysics.CastVoxel(_mainCamera.Entity.Transform.Position, _mainCamera.Entity.Transform.Forward, 5, out var output))
            {
                output.Chunk.SetBlock((int)output.BlockPosition.X, (int)output.BlockPosition.Y, (int)output.BlockPosition.Z, null);

                output.Chunk.RebuildMeshAsync();
            }
        }
    }

    public override void Tick(float deltaTime)
    {
        if (ChunkPhysics.CastVoxel(_mainCamera.Entity.Transform.Position, _mainCamera.Entity.Transform.Forward, 5, out var output))
        {
            debugCube.Transform.Position = output.WorldPosition - new Vector3D<float>(0.0125f, 0.0125f, 0.0125f);
        }
        else
        {
            debugCube.Transform.Position = Silk.NET.Maths.Vector3D<float>.Zero;
        }
    }
}
