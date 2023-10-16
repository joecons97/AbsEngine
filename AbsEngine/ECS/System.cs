namespace AbsEngine.ECS;

public abstract class System : IDisposable
{
    public Scene Scene { get; private set; }

    protected bool isDisposed = false;

    internal bool isFrameComplete { get; set; }

    protected System(Scene scene)
    {
        Scene = scene;
    }

    public virtual void OnStart() { }

    public virtual void Tick(float deltaTime) { }

    public void Dispose()
    {
        isDisposed = true;
    }
}
