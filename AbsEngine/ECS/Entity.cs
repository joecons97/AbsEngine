using AbsEngine.ECS.Components;

namespace AbsEngine.ECS;

public class Entity
{
    public Scene Scene { get; }
    public ComponentList Components { get; }
    public uint ID { get; }

    private TransformComponent _cachedTransform = null!;

    public TransformComponent Transform
    {
        get
        {
            if (_cachedTransform == null)
                _cachedTransform = GetComponent<TransformComponent>()!;

            return _cachedTransform;
        }
    }

    internal Entity(uint id, Scene scene)
    {
        ID = id;
        Components = new ComponentList();
        Scene = scene;
    }

    public T? GetComponent<T>() where T : Component
    {
        var type = typeof(T);
        if (Components.ContainsKey(type) == false)
            return null;

        return (T)Components[type].First();
    }

    public T AddComponent<T>(params object?[] ctr) where T : Component
    {
        return Scene.EntityManager.CreateComponent<T>(this, ctr);
    }

    internal void AddComponent(Component component)
    {
        Components.Add(component);
    }
}
