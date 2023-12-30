namespace AbsEngine.ECS;

public abstract class System : IDisposable
{
    public Scene Scene { get; private set; }

    protected bool isDisposed = false;

    protected System(Scene scene)
    {
        Scene = scene;
    }

    public virtual void OnStart() { }

    public virtual void Tick(float deltaTime) 
    {
    }

    public virtual void OnGui(float deltaTime) 
    { 
    }

    public void Dispose()
    {
        isDisposed = true;
    }
}
