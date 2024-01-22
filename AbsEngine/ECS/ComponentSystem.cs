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
            IEnumerable<T> initQuery;
            using (Profiler.BeginEvent($"Scene.EntityManager.GetComponents<{typeof(T)}>"))
                initQuery = Scene.EntityManager.GetComponents<T>(out count);

            using (Profiler.BeginEvent($"Skip"))
            {
                if (_componentsToSkip > 0)
                    initQuery = initQuery.Skip(_componentsToSkip);
            }

            using (Profiler.BeginEvent($"Take"))
            {
                if (MaxIterationsPerFrame != int.MaxValue)
                    initQuery = initQuery.Take(MaxIterationsPerFrame);
            }

            using (Profiler.BeginEvent($"ToList"))
            {
                comps = initQuery.ToList();
            }

            using (Profiler.BeginEvent("Re-count"))
            {
                if (MaxIterationsPerFrame != int.MaxValue && (_componentsToSkip += MaxIterationsPerFrame) > count)
                    _componentsToSkip = 0;

                count = comps.Count;
            }
        }

        OnInitialiseTick(deltaTime);

        for (int i = 0; i < comps.Count; i++)
        {
            T? comp = comps[i];

            using (Profiler.BeginEvent($"Tick {comp.Entity.Name}"))
                OnTick(comp, deltaTime);
        }
    }

    public abstract void OnTick(T component, float deltaTime);
}
