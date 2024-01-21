using AbsEngine.ECS;
using Assimp;
using Schedulers;

namespace AbsEngine.Jobs
{
    internal class ComponentsJob<T> : IJobParallelFor where T : Component
    {
        public int ThreadCount { get; }

        public int BatchSize => 1;

        ComponentSystem<T> _system;

        IReadOnlyCollection<T> _components;

        float _dt;

        internal ComponentsJob(ComponentSystem<T> system, IReadOnlyCollection<T> components, float dt)
        {
            _system = system;
            _components = components;
            _dt = dt;
        }

        public void Execute(int index)
        {
            using (Profiler.BeginEvent($"{_system} OnTick"))
                _system.OnTick(_components.ElementAt(index), _dt);
        }

        public void Finish()
        {

        }
    }
}
