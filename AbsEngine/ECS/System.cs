using Schedulers;

namespace AbsEngine.ECS;

public abstract class System : IDisposable
{
    public Scene Scene { get; private set; }

    protected bool isDisposed = false;

    protected virtual float? FixedTimeStep => null;

    public virtual bool UseJobSystem => true;

    float lastTickTime = 0;

    protected System(Scene scene)
    {
        Scene = scene;
    }

    public virtual void OnStart() { }

    public JobHandle? Tick(float deltaTime)
    {
        if (FixedTimeStep != null)
        {
            if (lastTickTime >= FixedTimeStep.Value)
            {
                lastTickTime = 0;
                deltaTime = Math.Max(FixedTimeStep.Value, deltaTime);
            }
            else
            {
                lastTickTime += deltaTime;
                return null;
            }
        }
        if (UseJobSystem)
        {
            using (Profiler.BeginEvent("Create Job"))
            {
                var job = new SystemTickJob(this, deltaTime);
                return Scene.Game.Scheduler.Schedule(job);
            }
        }
        else
        {
            OnTick(deltaTime);

            return null;
        }
    }

    public virtual void OnTick(float deltaTime)
    {
    }

    public virtual void OnGui(float deltaTime) 
    { 
    }

    public void Dispose()
    {
        isDisposed = true;
    }

    class SystemTickJob : IJob
    {
        public System System { get; }
        public float DeltaTime { get; }

        public SystemTickJob(System system, float deltaTime)
        {
            System = system;
            DeltaTime = deltaTime;
        }

        public void Execute()
        {
            using (Profiler.BeginEvent($"{System.GetType()}.OnTick"))
            {
                System.OnTick(DeltaTime);
            }
        }
    }

}
