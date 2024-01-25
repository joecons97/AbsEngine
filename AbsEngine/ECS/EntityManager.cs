using AbsEngine.Collections;
using AbsEngine.ECS.Components;
using System;
using System.Collections.Concurrent;

namespace AbsEngine.ECS;

public class EntityManager
{
    public Scene Scene { get; }

    private Dictionary<Type, UnsafeArrayList> _components = new Dictionary<Type, UnsafeArrayList>();
    private HashSet<Entity> _entities = new HashSet<Entity>();

    public EntityManager(Scene scene)
    {
        Scene = scene;
    }

    internal void AddComponent(Component component)
    {
        var type = component.GetType();

        if (_components.ContainsKey(type) == false)
            _components.Add(type, new UnsafeArrayList() { component });
        else
        {
            lock(_components[type])
                _components[type].Add(component);
        }
    }

    internal T[] GetComponentsUnsafe<T>() where T : Component
    {
        using (Profiler.BeginEvent($"GetComponentsUnsafe<{typeof(T)}>"))
        {
            var type = typeof(T);
            if (_components.ContainsKey(type) == false)
                return Array.Empty<T>();

            lock (_components[type])
                return _components[type].UnsafeConvert<T>();
        }
    }

    public T[] GetComponents<T>() where T : Component
    {
        using (Profiler.BeginEvent($"GetComponents<{typeof(T)}>"))
        {
            var type = typeof(T);
            if (_components.ContainsKey(type) == false)
                return Array.Empty<T>();

            lock (_components[type])
                return _components[type].ToArray<T>()!;
        }
    }

    public T? GetFirstOrDefault<T>(Func<T, bool> predicate) where T : Component
    {
        using (Profiler.BeginEvent($"GetFirstOrDefault<{typeof(T)}>"))
        {
            return GetComponentsUnsafe<T>().Where(x => x != null).FirstOrDefault(predicate);
        }
    }

    public T CreateComponent<T>(Entity entity, params object?[] ctr) where T : Component
    {
        var type = typeof(T);

        var component = (T)Activator.CreateInstance(type, ctr)!;

        component.SetEntity(entity);
        entity.Components.Add(component);

        if (_components.ContainsKey(type) == false)
            _components.Add(type, new UnsafeArrayList() { component });
        else
        {
            lock (_components[type])
                _components[type].Add(component);
        }

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
