using AbsEngine.Rendering.OpenGL;
using Silk.NET.Maths;

namespace AbsEngine.Rendering;

internal enum GlobalShaderVariableType
{
    Int,
    Uint,
    Float,
    Vector2,
    Vector3,
    Vector4,
    Matrix
}

internal struct GlobalShaderVariable
{
    public GlobalShaderVariableType Type;
    public object Value;
}

internal interface IBackendShader : IDisposable
{
    public void Bind();

    public void LoadFromString(string str);

    public void SetInt(string name, int value);
    public void SetUint(string name, uint value);
    public void SetFloat(string name, float value);
    public void SetVector(string name, Vector4D<float> value);
    public void SetMatrix(string name, Matrix4X4<float> value);
    public void SetTexture(string name, IBackendTexture texture);

    public int GetRenderQueuePosition();
}

public class Shader : IDisposable
{
    internal IBackendShader _backendShader = null!;

    internal static Dictionary<string, GlobalShaderVariable> _globalVariables = new Dictionary<string, GlobalShaderVariable>();

    public bool IsTransparent
        => _backendShader.GetRenderQueuePosition() == Renderer.TRANSPARENT_QUEUE_POSITION;

    public Shader()
    {
        _backendShader = Game.Instance!.Graphics.GraphicsAPIs switch
        {
            GraphicsAPIs.OpenGL => new OpenGLShader(),
            GraphicsAPIs.D3D11 => throw new NotImplementedException(),
            _ => throw new NotImplementedException(),
        };
    }

    internal void ApplyBackendShader(IBackendShader shader)
        => _backendShader = shader;

    internal IBackendShader GetBackendShader()
        => _backendShader;

    public void Bind()
        => _backendShader?.Bind();

    public void Dispose()
    {
        Game.Instance?.QueueDisposable(_backendShader);

        GC.SuppressFinalize(this);
    }

    ~Shader() => Dispose();

    public void SetInt(string name, int value)
        => _backendShader?.SetInt(name, value);
    public void SetUint(string name, uint value)
        => _backendShader?.SetUint(name, value);
    public void SetFloat(string name, float value)
        => _backendShader?.SetFloat(name, value);
    public void SetVector(string name, Vector4D<float> value)
        => _backendShader?.SetVector(name, value);
    public void SetMatrix(string name, Matrix4X4<float> value)
        => _backendShader?.SetMatrix(name, value);
    public void SetTexture(string name, Texture texture)
        => _backendShader.SetTexture(name, texture.GetBackendTexture());

    public static void SetGlobalGlobalInt(string name, int value)
        => SetGlobalVariable(name, value, GlobalShaderVariableType.Int);
    public static void SetGlobalUint(string name, uint value)
        => SetGlobalVariable(name, value, GlobalShaderVariableType.Uint);
    public static void SetGlobalFloat(string name, float value)
        => SetGlobalVariable(name, value, GlobalShaderVariableType.Float);
    public static void SetGlobalVector(string name, Vector4D<float> value)
        => SetGlobalVariable(name, value, GlobalShaderVariableType.Vector4);
    public static void SetGlobalVector(string name, Vector3D<float> value)
        => SetGlobalVariable(name, value, GlobalShaderVariableType.Vector3);
    public static void SetGlobalVector(string name, Vector2D<float> value)
        => SetGlobalVariable(name, value, GlobalShaderVariableType.Vector2);
    public static void SetGlobalMatrix(string name, Matrix4X4<float> value)
        => SetGlobalVariable(name, value, GlobalShaderVariableType.Matrix);
    
    static void SetGlobalVariable(string name, object value, GlobalShaderVariableType type)
    {
        if(_globalVariables.ContainsKey(name))
            _globalVariables[name] = new GlobalShaderVariable() { Type = type, Value = value };
        else
            _globalVariables.Add(name, new GlobalShaderVariable() { Type = type, Value = value });
    }
}
