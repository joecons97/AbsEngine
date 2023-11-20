namespace AbsEngine.ECS;

public class Component
{
    public Entity Entity { get; internal set; } = null!;

    internal void SetEntity(Entity entity)
    {
        Entity = entity;
    }

    public virtual void OnStart() { }
}
