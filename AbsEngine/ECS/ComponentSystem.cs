namespace AbsEngine.ECS;

public abstract class ComponentSystem<T> : System where T : Component
{
    protected virtual Func<T, bool>? Predicate => null;

    protected virtual bool UseParallel => false;

    protected virtual int MaxIterationsPerFrame => int.MaxValue;
    protected virtual float? FixedTimeStep => null;

    float lastTickTime = 0;

    protected ComponentSystem(Scene scene) : base(scene)
    {
    }

    public virtual void OnInitialiseTick(float deltaTime) { }

    public override void Tick(float deltaTime)
    {
        if (FixedTimeStep != null)
        {
            if (lastTickTime >= FixedTimeStep.Value)
            {
                lastTickTime = 0;
                deltaTime = FixedTimeStep.Value;
            }
            else
            {
                lastTickTime += deltaTime;
                return;
            }
        }

        var comps = Predicate == null
            ? Scene.EntityManager.GetComponents<T>()
            : Scene.EntityManager.GetComponents(Predicate);

        comps = comps.Take(MaxIterationsPerFrame);

        OnInitialiseTick(deltaTime);

        if (UseParallel)
        {
            Parallel.ForEach(comps, t =>
            {
                OnTick(t, deltaTime);
            });
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
