using AbsEngine.ECS.Components;
using System;
using System.Collections.Concurrent;

namespace AbsEngine.ECS;

public class EntityManager
{
    public Scene Scene { get; }

    private Dictionary<Type, ConcurrentBag<Component>> _components = new Dictionary<Type, ConcurrentBag<Component>>();
    private HashSet<Entity> _entities = new HashSet<Entity>();

    public EntityManager(Scene scene)
    {
        Scene = scene;
    }

    internal void AddComponent(Component component)
    {
        var type = component.GetType();

        if (_components.ContainsKey(type) == false)
            _components.Add(type, new ConcurrentBag<Component>() { component });
        else
            _components[type].Add(component);
    }

    public IReadOnlyCollection<T> GetComponents<T>(int count = 0) where T : Component
    {
        var type = typeof(T);
        if (_components.ContainsKey(type) == false)
            return new List<T>(0);

        var bag = _components[type];
        List<T> result = new List<T>();

        using (Profiler.BeginEvent("GetComponents Loop"))
        {
            foreach (var component in bag)
            {
                var cast = (T)component;
                result.Add(cast);
                if (count > 0 && result.Count >= count)
                    break;
            }
        }

        return result;
    }

    public IReadOnlyCollection<T> GetComponents<T>(Func<T, bool> predicate, int count = 0) where T : Component
    {
        var type = typeof(T);
        if (_components.ContainsKey(type) == false)
            return new List<T>(0);

        var bag = _components[type];
        List<T> result = new List<T>();

        using (Profiler.BeginEvent("GetComponents Loop"))
        {
            foreach (var component in bag)
            {
                var cast = (T)component;
                bool res;
                using (Profiler.BeginEvent("Execute Predicate"))
                    res = predicate.Invoke(cast);

                if (res)
                {
                    result.Add(cast);
                    if (count > 0 && result.Count >= count)
                        break;
                }
            }
        }

        return result;
    }

    public IReadOnlyCollection<Component> GetComponentListReference<T>() where T : Component
    {
        var type = typeof(T);
        if (_components.ContainsKey(type) == false)
            _components.Add(type, new ConcurrentBag<Component>());

        return _components[type];
    }

    public T CreateComponent<T>(Entity entity, params object?[] ctr) where T : Component
    {
        var type = typeof(T);

        var component = (T)Activator.CreateInstance(type, ctr)!;

        component.SetEntity(entity);
        entity.Components.Add(component);

        if (_components.ContainsKey(type) == false)
            _components.Add(type, new ConcurrentBag<Component>() { component });
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
