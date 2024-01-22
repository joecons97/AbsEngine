namespace AbsEngine.ECS;

public abstract class AsyncComponentSystem<T> : System where T : Component
{
    protected virtual int MaxIterationsPerFrame => int.MaxValue;

    private List<T> _componentsInProgress = new List<T>();

    protected AsyncComponentSystem(Scene scene) : base(scene)
    {
    }

    public override void OnTick(float deltaTime)
    {
        Task.Run(async () =>
        {
            var comps = Scene.EntityManager.GetComponents<T>().Take(MaxIterationsPerFrame);

            await Parallel.ForEachAsync(comps, async (comp, token) =>
            {
                if (_componentsInProgress.Contains(comp) == false)
                {
                    _componentsInProgress.Add(comp);

                    await OnTickAsync(comp, deltaTime);

                    _componentsInProgress.Remove(comp);
                }
            });
        });
    }

    public abstract Task OnTickAsync(T component, float deltaTime);
}
