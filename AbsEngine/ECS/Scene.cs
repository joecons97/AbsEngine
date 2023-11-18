using AbsEngine.Exceptions;
using AbsEngine.IO;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;

namespace AbsEngine.ECS;

public class Scene : IDisposable
{
    public string Name { get; set; }
    public Game Game { get; private set; }
    public EntityManager EntityManager { get; }

    private List<System> _systems = new List<System>();

    private Dictionary<Guid, object> _assets = new Dictionary<Guid, object>();

    internal bool _hasTickBegun = false;

    internal float _deltaTime;

    internal Scene(string name)
    {
        if (Game.Instance == null)
            throw new GameInstanceException();

        Game = Game.Instance;

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
        var scene = new Scene("New Scene");

        scene.Game._activeScenes.Add(scene);    

        return scene;
    }

    public static Scene Load(string fileLocation)
    {
        var scene = SceneLoader.LoadSceneFromFile(fileLocation);

        scene.Game._activeScenes.Add(scene);

        return scene;
    }

    public void Tick(float deltaTime)
    {
        _hasTickBegun = true;
        _deltaTime = deltaTime;

        foreach(var system in _systems)
        {
            system.Tick(deltaTime);
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
