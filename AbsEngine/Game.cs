using AbsEngine.Rendering;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace AbsEngine;

public class Game
{
    public static Game? Instance { get; private set; }

    public event Action? OnLoad;
    public event Func<double, Task>? OnUpdate;
    public event Func<double, Task>? OnRender;

    public IInputContext InputContext { get; private set; } = null!;
    public IGraphics Graphics { get; private set; } = null!;
    public IWindow Window { get => _window; }

    private string _organsition;
    private string _name;
    private Vector2D<int> _size;

    private WindowOptions _windowOptions;
    private IWindow _window;

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
            VSync = false
        };

        _window = Silk.NET.Windowing.Window.Create(_windowOptions);

        _window.Load += () =>
        {
            Graphics = IGraphics.Create(_window, gfxApi);
            InputContext = _window.CreateInput();

            Graphics.SetActiveDepthTest(true);

            OnLoad?.Invoke();
        };

        _window.Update += (dt) =>
        {
            OnUpdate?.Invoke(dt).Wait();
        };
        _window.Render += (dt) =>
        {
            OnRender?.Invoke(dt).Wait();
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
}