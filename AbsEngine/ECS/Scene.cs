using AbsEngine.IO;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;

namespace AbsEngine.ECS;

public class Scene : IDisposable
{
    public string Name { get; set; }

    public EntityManager EntityManager { get; }

    private List<System> _systems = new List<System>();

    private Dictionary<Guid, object> _assets = new Dictionary<Guid, object>();

    internal bool _hasTickBegun = false;

    internal float _deltaTime;

    internal Scene(string name)
    {
        Name = name;

        EntityManager = new EntityManager(this);
    }

    internal void AddAsset(Guid guid, object asset)
    {
        _assets.Add(guid, asset);   
    }

    internal T GetAsset<T>(Guid guid)
    {
        return (T)_assets[guid];
    }

    public static Scene Load()
    {
        return new Scene("New Scene");
    }

    public static Scene Load(string fileLocation)
    {
        return SceneLoader.LoadSceneFromFile(fileLocation);
    }

    public async Task Tick(float deltaTime)
    {
        _hasTickBegun = true;
        _deltaTime = deltaTime;

        foreach(var system in _systems)
        {
            system.Tick(deltaTime);

            while(system.isFrameComplete == false)
                await Task.Yield();
        }

        _hasTickBegun = false;
    }

    public void RegisterSystem<T>(params object?[] ctr) where T : System
    {
        var type = typeof(T);

        var list = ctr.ToList();
        list.Insert(0, this);

        var system = (T)Activator.CreateInstance(type, list.ToArray())!;

        _systems.Add(system);

        system.OnStart();
    }

    internal System RegisterSystem(Type type, params object?[] ctr)
    {
        var list = ctr.ToList();
        list.Insert(0, this);

        var system = (System)Activator.CreateInstance(type, list.ToArray())!;

        _systems.Add(system);

        system.OnStart();

        return system;
    }

    public void Dispose()
    {
        foreach (var system in _systems)
        {
            system.Dispose();
        }
    }
}
