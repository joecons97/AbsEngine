namespace AbsEngine.ECS;

public abstract class ComponentSystem<T> : System where T : Component
{
    protected virtual int MaxIterationsPerFrame => int.MaxValue;

    private int _componentsToSkip = 0;

    protected ComponentSystem(Scene scene) : base(scene)
    {
    }

    public virtual void OnInitialiseTick(float deltaTime) { }

    public override void OnTick(float deltaTime)
    {
        List<T> comps;
        int count = 0;
        using (Profiler.BeginEvent("Gather Components"))
        {
            using (Profiler.BeginEvent($"Scene.EntityManager.GetComponents<{typeof(T)}>"))
                comps = Scene.EntityManager.GetComponents<T>().ToList();

            count = comps.Count;

            using (Profiler.BeginEvent($"Skip & Take"))
            {
                var list = comps.Skip(_componentsToSkip).Take(MaxIterationsPerFrame).ToList();
                comps = list;
            }

            using (Profiler.BeginEvent("Re-count"))
            {
                if (++_componentsToSkip > count)
                    _componentsToSkip = 0;

                count = comps.Count;
            }
        }

        OnInitialiseTick(deltaTime);

        for (int i = 0; i < comps.Count; i++)
        {
            T? comp = comps[i];
            OnTick(comp, deltaTime);
        }
    }

    public abstract void OnTick(T component, float deltaTime);
}
