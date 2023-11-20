using AbsEngine.ECS;
using AbsEngine.ECS.Components;

namespace AbsGameProject.Player
{
    public class PlayerControllerComponent : Component
    {
        public TransformComponent? CameraEntityTransform;

        public float LookSensitivity = 1;
        public float WalkSpeed = 1;
        public float RunSpeed = 2;

        public override void OnStart()
        {
            CameraEntityTransform = Entity.Transform.GetChild("Player Camera");
        }
    }
}
