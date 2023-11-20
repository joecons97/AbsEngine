using AbsEngine.ECS.Components;

namespace AbsEngine.ECS;

public class EntityManager
{
    public Scene Scene { get; }

    private Dictionary<Type, HashSet<Component>> _components = new Dictionary<Type, HashSet<Component>>();
    private HashSet<Entity> _entities = new HashSet<Entity>();

    public EntityManager(Scene scene)
    {
        Scene = scene;
    }

    internal void AddComponent(Component component)
    {
        var type = component.GetType();

        if (_components.ContainsKey(type) == false)
            _components.Add(type, new HashSet<Component>() { component });
        else
            _components[type].Add(component);
    }

    public IEnumerable<T> GetComponents<T>() where T : Component
    {
        var type = typeof(T);
        if(_components.ContainsKey(type) == false)
            return Enumerable.Empty<T>();

        return _components[type].Select(x => (T)x);
    }

    public IEnumerable<T> GetComponents<T>(Func<T, bool> predicate) where T : Component
    {
        var type = typeof(T);
        if (_components.ContainsKey(type) == false)
            return Enumerable.Empty<T>();

        return _components[type].Select(x => (T)x).Where(predicate);
    }

    public T CreateComponent<T>(Entity entity, params object?[] ctr) where T : Component
    {
        var type = typeof(T);

        var component = (T)Activator.CreateInstance(type, ctr)!;

        component.SetEntity(entity);
        entity.Components.Add(component);

        if (_components.ContainsKey(type) == false)
            _components.Add(type, new HashSet<Component>() { component });
        else
            _components[type].Add(component);

        component.OnStart();

        return component;
    }

    public Entity CreateEntity(string name = "New Entity")
    {
        var ent = new Entity((uint)_entities.Count, Scene);
        _entities.Add(ent);

        CreateComponent<TransformComponent>(ent);
        CreateComponent<NameComponent>(ent);

        ent.Name = name;

        return ent;
    }

    internal Entity CreateEmptyEntity()
    {
        var ent = new Entity((uint)_entities.Count, Scene);
        _entities.Add(ent);

        return ent;
    }
}
