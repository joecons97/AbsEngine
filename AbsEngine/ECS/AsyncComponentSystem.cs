namespace AbsEngine.ECS;

public abstract class AsyncComponentSystem<T> : System where T : Component
{
    protected virtual Func<T, bool>? Predicate => null;

    protected AsyncComponentSystem(Scene scene) : base(scene)
    {
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
        });
    }

    public abstract Task OnTickAsync(T component, float deltaTime);
}
