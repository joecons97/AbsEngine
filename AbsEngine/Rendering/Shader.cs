using AbsEngine.Rendering.OpenGL;
using Silk.NET.Maths;

namespace AbsEngine.Rendering;

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
}

public class Shader : IDisposable
{
    private IBackendShader _backendShader = null!;

    public Shader()
    {
        switch (Game.Instance!.Graphics.GraphicsAPIs)
        {
            case GraphicsAPIs.OpenGL:
                _backendShader = new OpenGLShader();
                break;
            case GraphicsAPIs.D3D11:
                _backendShader = null!;
                throw new NotImplementedException();
        }
    }

    internal void ApplyBackendShader(IBackendShader shader)
        => _backendShader = shader;

    internal IBackendShader GetBackendShader()
        => _backendShader;

    public void Bind()
        => _backendShader?.Bind();

    public void Dispose()
    { 
        _backendShader?.Dispose();

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
}
