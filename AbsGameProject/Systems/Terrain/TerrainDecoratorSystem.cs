using AbsEngine.ECS;
using AbsGameProject.Blocks;
using AbsGameProject.Components.Terrain;
using AbsGameProject.Structures;

namespace AbsGameProject.Systems.Terrain
{
    public class TerrainDecoratorSystem : ComponentSystem<TerrainChunkComponent>
    {
        protected override Func<TerrainChunkComponent, bool>? Predicate => (x) => x.IsReadyForDecoration;
        protected override int MaxIterationsPerFrame => 1;
        protected override bool UseParallel => true;

        Decorator test;
        Random random;

        public TerrainDecoratorSystem(Scene scene) : base(scene)
        {
            test = new TreeDecorator(BlockRegistry.GetBlock("log_oak"), BlockRegistry.GetBlock("leaves_oak"), 5, 2, 2);
            random = new Random(1);
        }

        public override void OnTick(TerrainChunkComponent component, float deltaTime)
        {
            _ = Task.Run(async () =>
            {
                var decorationTasks = new List<Task>(); 
                for(int x = 0; x < TerrainChunkComponent.WIDTH; x +=2)
                {
                    for (int z = 0; z < TerrainChunkComponent.WIDTH; z+=2)
                    {
                        var y = component.GetHeight(x, z);
                        var block = component.GetBlock(x, y - 1, z);
                        if (block.Name == "Grass" && random.NextDouble() < 0.02f)
                            decorationTasks.Add(test.DecorateAtAsync(component, x, y, z));
                    }
                }

                await Task.WhenAll(decorationTasks);

                component.State = TerrainChunkComponent.TerrainState.Decorated;
            });
        }
    }
}
