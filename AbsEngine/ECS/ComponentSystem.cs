namespace AbsEngine.ECS;

public abstract class ComponentSystem<T> : System where T : Component
{

    protected virtual bool UseParallel => false;

    protected virtual int MaxIterationsPerFrame => int.MaxValue;

    private int _componentsToSkip = 0;

    protected ComponentSystem(Scene scene) : base(scene)
    {
    }

    public virtual void OnInitialiseTick(float deltaTime) { }

    public override void OnTick(float deltaTime)
    {
        IReadOnlyCollection<T> comps;
        int count = 0;
        using (Profiler.BeginEvent("Gather Components"))
        {
            using (Profiler.BeginEvent($"Scene.EntityManager.GetComponents<{typeof(T)}>"))
                comps = Scene.EntityManager.GetComponents<T>();

            count = comps.Count;

            using (Profiler.BeginEvent($"Skip & Take"))
            {
                comps = comps.Skip(_componentsToSkip).Take(MaxIterationsPerFrame).ToList();
            }

            using (Profiler.BeginEvent("Re-count"))
            {
                if (++_componentsToSkip > count)
                    _componentsToSkip = 0;

                count = comps.Count;
            }
        }

        OnInitialiseTick(deltaTime);

        if (UseParallel)
        {
            if (count > 0)
            {
                Parallel.ForEach(comps, t =>
                {
                    OnTick(t, deltaTime);
                });
            }
        }
        else
        {
            foreach (var comp in comps)
            {
                OnTick(comp, deltaTime);
            }
        }
    }

    public abstract void OnTick(T component, float deltaTime);
}
