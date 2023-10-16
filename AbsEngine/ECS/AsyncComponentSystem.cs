namespace AbsEngine.ECS;

public abstract class AsyncComponentSystem<T> : System where T : Component
{
    bool canTick = false;

    protected virtual Func<T, bool>? Predicate => null;

    protected AsyncComponentSystem(Scene scene) : base(scene)
    {
    }

    public async Task NextTick()
    {
        isFrameComplete = true;

        while (canTick == false)
            await Task.Yield();

        canTick = false;
    }

    public override void Tick(float deltaTime)
    {
        Task.Run(async () =>
        {
            var comps = Predicate == null
                ? Scene.EntityManager.GetComponents<T>()
                : Scene.EntityManager.GetComponents(Predicate);

            await Parallel.ForEachAsync(comps, async (comp, token) =>
            {
                await OnTickAsync(comp, Scene._deltaTime);
            });

            isFrameComplete = true;
            canTick = true;
        });
    }

    public abstract Task OnTickAsync(T component, float deltaTime);
}
