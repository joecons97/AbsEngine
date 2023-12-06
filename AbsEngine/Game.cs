using AbsEngine.ECS;
using AbsEngine.IO;
using AbsEngine.Rendering;
using AbsEngine.Rendering.OpenGL;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using System.Collections.Concurrent;

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

    public float DeltaTime { get; private set; }
    public float Time { get; private set; }

    public IReadOnlyList<Scene> ActiveScenes { get => _activeScenes; }

    private string _organsition;
    private string _name;
    private Vector2D<int> _size;

    private WindowOptions _windowOptions;
    private IWindow _window;

    private ImGuiController? _imGuiController;

    private ConcurrentQueue<IDisposable> _queueForDisposal = new ConcurrentQueue<IDisposable>();

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

            if (gfxApi == GraphicsAPIs.OpenGL)
            {
                _imGuiController = new ImGuiController(
                    ((OpenGLGraphics)Graphics).Gl, // load OpenGL
                    _window, // pass in our window
                    InputContext // create an input context
                );
            }
            Graphics.SetActiveDepthTest(true);

            ShaderLoader.ScanShaders();

            OnLoad?.Invoke();
        };

        _window.Update += (dt) =>
        {
            DeltaTime = (float)dt;
            Time += DeltaTime;
            int count = 0;
            while (_queueForDisposal.Count > 0)
            {
                if(!_queueForDisposal.TryDequeue(out var elm))
                {
                    continue;
                }

                elm.Dispose();

                count++;

                if (count > MAX_DISPOSABLE_PER_FRAME)
                    break;
            }

            foreach (var item in _activeScenes)
            {
                item.Tick((float)dt);
            }

            OnUpdate?.Invoke(dt);

            _imGuiController?.Update((float)dt);
        };

        _window.Render += (dt) =>
        {
            Renderer.CompleteFrame();

            OnRender?.Invoke(dt);

            _imGuiController?.Render();
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