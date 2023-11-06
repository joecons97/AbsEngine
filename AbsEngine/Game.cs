using AbsEngine.ECS;
using AbsEngine.IO;
using AbsEngine.Rendering;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace AbsEngine;

public class Game
{
    const int MAX_DISPOSABLE_PER_FRAME = 10;

    public static Game? Instance { get; private set; }

    public event Action? OnLoad;
    public event Action<double>? OnUpdate;
    public event Action<double>? OnRender;

    public IInputContext InputContext { get; private set; } = null!;
    public IGraphics Graphics { get; private set; } = null!;
    public IWindow Window { get => _window; }

    private string _organsition;
    private string _name;
    private Vector2D<int> _size;

    private WindowOptions _windowOptions;
    private IWindow _window;

    private Queue<IDisposable> _queueForDisposal = new Queue<IDisposable>();

    internal List<Scene> _activeScenes = new();

    public Game(string org, string name, GraphicsAPIs gfxApi, Vector2D<int> size = default)
    {
        if (Instance != null)
            throw new InvalidOperationException("There must only ever be one instance of \"Game\"");

        Instance = this;

        _organsition = org;
        _name = name;
        //if(size == default)
        //TODO Load from file

        _size = size;

        _windowOptions = WindowOptions.Default with
        {
            Size = _size,
            Title = _name,
            VSync = false,
            API = gfxApi == GraphicsAPIs.OpenGL ? GraphicsAPI.Default : GraphicsAPI.None
        };

        _window = Silk.NET.Windowing.Window.Create(_windowOptions);

        _window.Load += () =>
        {
            Graphics = IGraphics.Create(_window, gfxApi);
            InputContext = _window.CreateInput();

            Graphics.SetActiveDepthTest(true);

            ShaderLoader.ScanShaders();

            OnLoad?.Invoke();
        };

        _window.Update += (dt) =>
        {
            int count = 0;
            while (_queueForDisposal.Count > 0)
            {
                _queueForDisposal.Dequeue().Dispose();
                count++;

                if (count > MAX_DISPOSABLE_PER_FRAME)
                    break;
            }

            foreach (var item in _activeScenes)
            {
                item.Tick((float)dt);
            }

            OnUpdate?.Invoke(dt);
        };

        _window.Render += (dt) =>
        {
            Graphics.ClearScreen(System.Drawing.Color.CornflowerBlue);

            Rendering.Renderer.CompleteFrame();

            OnRender?.Invoke(dt);
        };

        _window.FramebufferResize += (s) =>
        {
            Graphics?.UpdateViewport(s);
        };
    }

    public void Run()
    {
        _window.Run();
    }

    public void QueueDisposable(IDisposable disposable)
    {
        if (!_queueForDisposal.Contains(disposable))
            _queueForDisposal.Enqueue(disposable);
    }
}