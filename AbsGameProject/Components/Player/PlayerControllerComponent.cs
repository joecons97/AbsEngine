using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsGameProject.Maths.Physics;

namespace AbsGameProject.Components.Player
{
    public class PlayerControllerComponent : Component
    {
        public TransformComponent? CameraEntityTransform;
        public VoxelRigidbodyComponent? VoxelRigidbody;

        public float MaxLookUp = 80;
        public float MinLookUp = -80;
        public float CurrentLookUp = 0;

        public float LookSensitivity = 0.1f;
        public float WalkSpeed = 5;
        public float RunSpeed = 2;
        public float JumpStrength = 8;

        public bool IsGrounded = true;

        public override void OnStart()
        {
            CameraEntityTransform = Entity.Transform.GetChild("Player Camera");
            CameraEntityTransform.LocalPosition = new Silk.NET.Maths.Vector3D<float>(0, 1.8f, 0);
            VoxelRigidbody = Entity.GetComponent<VoxelRigidbodyComponent>();
        }
    }
}
