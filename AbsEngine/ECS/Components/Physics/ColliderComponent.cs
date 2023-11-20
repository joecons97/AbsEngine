using AbsEngine.Physics;

namespace AbsEngine.ECS.Components.Physics;

public abstract class ColliderComponent : Component
{
    public abstract IShape Shape { get; }
}
