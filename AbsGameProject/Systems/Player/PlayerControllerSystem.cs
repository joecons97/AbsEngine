using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsEngine.Rendering;
using AbsGameProject.Components.Player;
using AbsGameProject.Maths.Physics;
using AbsGameProject.Models;
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
        var cullable = CullableMesh.TryFromVoxelMesh(vox);
        var mesh = new Mesh();
        mesh.UseTriangles = false;
        mesh.Positions = cullable.Faces.Select(x => x.Value).SelectMany(x => x.Positions).ToArray();

        mesh.Build();

        _playerRenderer = _playerController.Entity.AddComponent<MeshRendererComponent>();
        _playerRenderer.Mesh = mesh;
        _playerRenderer.Material = new Material("NewSyntax");
    }

    private void _keyboard_KeyDown(IKeyboard arg1, Key arg2, int arg3)
    {
        if (arg2 == Key.Space && _playerController.IsGrounded)
            _playerController.VoxelRigidbody.AddImpluse(_playerController.JumpStrength * Vector3D<float>.UnitY);
    }

    public override void Tick(float deltaTime)
    {
        _playerRenderer.IsEnabled = !SceneCameraComponent.IsInSceneView;

        if (SceneCameraComponent.IsInSceneView)
            return;

        MouseLook();

        var t = _playerController.Entity.Transform;

        var velocity = new Vector3D<float>(0, _playerController.VoxelRigidbody.Velocity.Y, 0);

        var pos = _playerController.Entity.Transform.Position + Vector3D<float>.UnitY * 0.1f;
        _playerController.IsGrounded = ChunkPhysics.CastVoxel(pos, -Vector3D<float>.UnitY, 0.2f, out var _);

        if (_keyboard.IsKeyPressed(Key.W))
        {
            velocity += t.Forward * _playerController.WalkSpeed;
        }
        else if (_keyboard.IsKeyPressed(Key.S))
        {
            velocity += t.Forward * -_playerController.WalkSpeed;
        }

        if (_keyboard.IsKeyPressed(Key.D))
        {
            velocity += t.Right * -_playerController.WalkSpeed;
        }
        else if (_keyboard.IsKeyPressed(Key.A))
        {
            velocity += t.Right * _playerController.WalkSpeed;
        }
        if (_keyboard.IsKeyPressed(Key.Equal))
        {
            _mouse.Cursor.CursorMode = CursorMode.Normal;
            SceneCameraComponent.IsInSceneView = true;
        }

        _playerController.VoxelRigidbody.Velocity = velocity;
    }

    void MouseLook()
    {
        if (_playerController.CameraEntityTransform == null)
            return;

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
