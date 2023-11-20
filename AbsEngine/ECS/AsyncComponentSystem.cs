﻿namespace AbsEngine.ECS;

public abstract class AsyncComponentSystem<T> : System where T : Component
{
    protected virtual Func<T, bool>? Predicate => null;
    protected virtual int MaxIterationsPerFrame => int.MaxValue;
    protected virtual float? FixedTimeStep => null;

    float lastTickTime = 0;

    protected AsyncComponentSystem(Scene scene) : base(scene)
    {
    }

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

        Task.Run(async () =>
        {
            var comps = Predicate == null
                ? Scene.EntityManager.GetComponents<T>()
                : Scene.EntityManager.GetComponents(Predicate);

            comps = comps.Take(MaxIterationsPerFrame);

            await Parallel.ForEachAsync(comps, async (comp, token) =>
            {
                await OnTickAsync(comp, Scene._deltaTime);
            });
        });
    }

    public abstract Task OnTickAsync(T component, float deltaTime);
}
