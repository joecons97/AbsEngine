using AbsEngine.ECS;
using AbsEngine.ECS.Components;
using AbsGameProject.Components.Physics;

namespace AbsGameProject.Components.Player
{
    public class PlayerControllerComponent : Component
    {
        public TransformComponent? CameraEntityTransform;
        public VoxelRigidbodyComponent VoxelRigidbody = default!;

        public float MaxLookUp = 80;
        public float MinLookUp = -80;
        public float CurrentLookUp = 0;

        public float LookSensitivity = 0.1f;
        public float WalkSpeed = 4;
        public float RunSpeed = 6;
        public float JumpStrength = 8;

        public bool IsInWater;
        public bool IsFeetInWater;
        public float WaterWalkSpeed = 3f;
        public float WaterRiseSpeed = 60f;
        public float WaterDrag = 15;

        public bool IsGrounded = true;
        public bool CanJumpFromWater = false;

        public override void OnStart()
        {
            CameraEntityTransform = Entity.Transform.GetChild("Player Camera");

            if(CameraEntityTransform != null )  
                CameraEntityTransform.LocalPosition = new Silk.NET.Maths.Vector3D<float>(0, 1.8f, 0);

            VoxelRigidbody = Entity.GetComponent<VoxelRigidbodyComponent>()!;

            if (VoxelRigidbody == null)
                throw new Exception("No VoxelRigidbodyComponent was found on the Player Entity");
        }
    }
}
