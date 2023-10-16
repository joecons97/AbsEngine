namespace AbsEngine.ECS;

public class ComponentList : Dictionary<Type, HashSet<Component>>
{
    internal void Add(Component component)
    {
        var type = component.GetType();
        if (ContainsKey(type) == false)
        {
            this[type] = new HashSet<Component>() { component };
        }
        else
        {
            this[type].Add(component);  
        }
    }

    new private void Add(Type type, HashSet<Component> components)
    {
        base.Add(type, components);
    }
}
