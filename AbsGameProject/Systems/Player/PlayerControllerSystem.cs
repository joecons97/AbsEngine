using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsEngine.Rendering;
using AbsGameProject.Components.Player;
using AbsGameProject.Components.Terrain;
using AbsGameProject.Maths.Physics;
using AbsGameProject.Models.Meshing;
using Silk.NET.Input;
using Silk.NET.Maths;
using System.Numerics;

namespace AbsGameProject.Systems.Player;

public class PlayerControllerSystem : AbsEngine.ECS.System
{
    PlayerControllerComponent _playerController;

    Vector2 _lastMousePos;
    IMouse _mouse;
    IKeyboard _keyboard;

    MeshRendererComponent _playerRenderer;

    public PlayerControllerSystem(Scene scene) : base(scene)
    {
        _playerController = scene.EntityManager.GetComponents<PlayerControllerComponent>().First();
        _mouse = scene.Game.InputContext.Mice.First();
        _keyboard = scene.Game.InputContext.Keyboards.First();
        _keyboard.KeyDown += _keyboard_KeyDown;

        var vox = VoxelModel.TryFromFile("Content/Models/Blocks/Player.json");
        if (vox == null)
            throw new Exception("Unable to load VoxelModel");

        var cullable = CullableMesh.TryFromVoxelMesh(vox);
        if (cullable == null)
            throw new Exception("Unable to load CullableMesh");

        var mesh = new Mesh();
        mesh.UseTriangles = false;
        mesh.Positions = cullable.Faces.Select(x => x.Value).SelectMany(x => x.Positions).ToArray();

        mesh.Build();

        _playerRenderer = _playerController.Entity.AddComponent<MeshRendererComponent>();
        _playerRenderer.Mesh = mesh;
        _playerRenderer.Material = new Material("NewSyntax");
    }

    public override void OnGui(float deltaTime)
    {
        //ImGui.Begin("Player");
        //ImGui.Text($"Position: {_playerController.Entity.Transform.Position}");
        //ImGui.Text($"Chunk: {_playerController.Entity.Transform.Position.ToChunkPosition()}");
        //ImGui.Text($"Chunk Space: {_playerController.Entity.Transform.Position.ToChunkSpaceFloored()}");
        //ImGui.End();
    }

    private void _keyboard_KeyDown(IKeyboard arg1, Key arg2, int arg3)
    {
        if (arg2 == Key.Space && _playerController.IsGrounded)
        {
            _playerController.VoxelRigidbody?.AddImpluse(_playerController.JumpStrength * Vector3D<float>.UnitY);
        }
    }

    public override void OnTick(float deltaTime)
    {
        _playerRenderer.IsEnabled = SceneCameraComponent.IsInSceneView;

        if (SceneCameraComponent.IsInSceneView)
            return;

        MouseLook();

        var t = _playerController.Entity.Transform;

        var velocity = new Vector3D<float>(0, _playerController.VoxelRigidbody?.Velocity.Y ?? 0, 0);

        using (Profiler.BeginEvent("Figure out state"))
        {
            var chunk = TerrainChunkComponent.GetAt(Scene, _playerController.Entity.Transform.Position);
            if (chunk != null)
            {
                var chunkSpacePos = _playerController.Entity.Transform.Position.ToChunkSpaceFloored();
                var block = chunk.GetBlock((int)chunkSpacePos.X, (int)chunkSpacePos.Y + 1, (int)chunkSpacePos.Z);
                var feetBlock = chunk.GetBlock((int)chunkSpacePos.X, (int)chunkSpacePos.Y, (int)chunkSpacePos.Z);

                _playerController.IsInWater = block.Id == "water";
                _playerController.IsFeetInWater = feetBlock.Id == "water";
            }

            _playerController.VoxelRigidbody!.Drag = _playerController.IsFeetInWater ? _playerController.WaterDrag : 1;

        }

        using (Profiler.BeginEvent("Input"))
        {
            var speed = _playerController.IsFeetInWater ? _playerController.WaterWalkSpeed : _playerController.WalkSpeed;

            if (_keyboard.IsKeyPressed(Key.W))
            {
                velocity += t.Forward * speed;
            }
            else if (_keyboard.IsKeyPressed(Key.S))
            {
                velocity += t.Forward * -speed;
            }

            if (_keyboard.IsKeyPressed(Key.D))
            {
                velocity += t.Right * -speed;
            }
            else if (_keyboard.IsKeyPressed(Key.A))
            {
                velocity += t.Right * speed;
            }
            if (_keyboard.IsKeyPressed(Key.Equal))
            {
                _mouse.Cursor.CursorMode = CursorMode.Normal;
                SceneCameraComponent.IsInSceneView = true;
            }
        }

        _playerController.VoxelRigidbody.Velocity = velocity;

        if ((_playerController.IsInWater || _playerController.IsGrounded) && !_playerController.CanJumpFromWater)
        {
            _playerController.CanJumpFromWater = true;
        }

        if (!_playerController.IsInWater)
        {
            using (Profiler.BeginEvent("Not in water"))
            {
                var pos = _playerController.Entity.Transform.Position + Vector3D<float>.UnitY * 0.5f;
                _playerController.IsGrounded = ChunkPhysics.CastVoxel(pos, -Vector3D<float>.UnitY, .65f, out var _);

                if (_keyboard.IsKeyPressed(Key.Space)
                    && _playerController.IsFeetInWater && velocity.Y > 0
                    && _playerController.CanJumpFromWater && _playerController.IsGrounded == false)
                {
                    _playerController.CanJumpFromWater = false;
                    _playerController.VoxelRigidbody?.AddImpluse(_playerController.JumpStrength * Vector3D<float>.UnitY * 2);
                }
            }
        }
        else
        {
            _playerController.IsGrounded = false;

            if (_keyboard.IsKeyPressed(Key.Space))
            {
                _playerController.VoxelRigidbody.AddForce(_playerController.WaterRiseSpeed * Vector3D<float>.UnitY);
            }
        }

    }

    void MouseLook()
    {
        if (_playerController.CameraEntityTransform == null)
            return;

        using (Profiler.BeginEvent("Mouse Look"))
        {
            _mouse.Cursor.CursorMode = CursorMode.Disabled;
            var pos = _mouse.Position;
            if (_lastMousePos == default)
                _lastMousePos = pos;

            var mouseDelta = _lastMousePos - pos;
            _lastMousePos = pos;

            _playerController.CurrentLookUp += mouseDelta.Y * _playerController.LookSensitivity;
            _playerController.CurrentLookUp = MathF.Min(MathF.Max(_playerController.CurrentLookUp, _playerController.MinLookUp), _playerController.MaxLookUp);

            _playerController.CameraEntityTransform.LocalEulerAngles = new Vector3D<float>(_playerController.CurrentLookUp, 0, 0);
            _playerController.Entity.Transform.LocalEulerAngles += new Vector3D<float>(0, mouseDelta.X * _playerController.LookSensitivity, 0);
        }
    }
}
