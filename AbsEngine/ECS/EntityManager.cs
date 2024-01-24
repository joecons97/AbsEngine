using AbsEngine.ECS.Components;
using System;
using System.Collections.Concurrent;

namespace AbsEngine.ECS;

public class EntityManager
{
    public Scene Scene { get; }

    private Dictionary<Type, List<Component>> _components = new Dictionary<Type, List<Component>>();
    private HashSet<Entity> _entities = new HashSet<Entity>();

    public EntityManager(Scene scene)
    {
        Scene = scene;
    }

    internal void AddComponent(Component component)
    {
        var type = component.GetType();

        if (_components.ContainsKey(type) == false)
            _components.Add(type, new List<Component>() { component });
        else
        {
            lock(_components[type])
                _components[type].Add(component);
        }
    }

    public IReadOnlyList<T> GetComponents<T>() where T : Component
    {
        var type = typeof(T);
        if (_components.ContainsKey(type) == false)
            return new List<T>(0);

        lock (_components[type])
            return _components[type].Select(x => (T)x).ToList();
    }

    public IReadOnlyList<T> GetComponents<T>(out int count) where T : Component
    {
        var type = typeof(T);
        if (_components.ContainsKey(type) == false)
        {
            count = 0;
            return new List<T>(0);
        }

        var list = _components[type];
        lock (list)
        {
            count = list.Count;

            return list.Select(x => (T)x).ToList();
        }
    }

    public IReadOnlyList<T> GetComponents<T>(Func<T, bool> predicate) where T : Component
    {
        var type = typeof(T);
        if (_components.ContainsKey(type) == false)
            return new List<T>(0);

        lock (_components[type])
            return _components[type].Select(x => (T)x).Where(predicate).ToList(); ;
    }

    public IReadOnlyCollection<Component> GetComponentListReference<T>() where T : Component
    {
        var type = typeof(T);
        if (_components.ContainsKey(type) == false)
            _components.Add(type, new List<Component>());

        lock (_components[type])
            return _components[type];
    }

    public T CreateComponent<T>(Entity entity, params object?[] ctr) where T : Component
    {
        var type = typeof(T);

        var component = (T)Activator.CreateInstance(type, ctr)!;

        component.SetEntity(entity);
        entity.Components.Add(component);

        if (_components.ContainsKey(type) == false)
            _components.Add(type, new List<Component>() { component });
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
