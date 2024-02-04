namespace AbsEngine.ECS;

public class Component
{
    public Entity Entity { get; internal set; } = null!;
    public bool IsEnabled { get; set; } = true;

    internal void SetEntity(Entity entity)
    {
        Entity = entity;
    }

    public virtual void OnStart() { }
}
